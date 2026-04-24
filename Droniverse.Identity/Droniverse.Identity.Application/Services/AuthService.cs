using AutoMapper;
using BCrypt.Net;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.HttpClients;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Enums;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using Droniverse.Shared.Services.IServices;
using Droniverse.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Droniverse.Identity.Application.Services;

internal class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly AppSettings _appSettings;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AuthService> _logger;
    private readonly AcademyMicroserviceClient _academyMicroserviceClient;

    public AuthService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings,
        IOptions<AppSettings> appSettings,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<AuthService> logger,
        AcademyMicroserviceClient academyMicroserviceClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
        _appSettings = appSettings.Value;
        _currentUserService = currentUserService;
        _academyMicroserviceClient = academyMicroserviceClient;
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
    }
    public async Task<AuthResponse> RefreshToken(string accessToken, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        string email = principal.Identity?.Name ?? throw new UnauthorizedAccessException("Invalid access token.");
        //check trong redis xem có bị blacklist hay không (cả access và refresh)

        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.Email == email);
        if (account is null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }
        if (account.RefreshToken != refreshToken ||
            account.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }
        UserResponse user = _mapper.Map<UserResponse>(account);
        string newAccessToken = GenerateAccessToken(account);
        string newRefreshToken = GenerateRefreshToken(account);
        account.RefreshToken = newRefreshToken;
        account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _unitOfWork.SaveChangeAsync();
        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            User = user
        };
    }
    public async Task<AuthResponse> RegisterUser(RegisterDto registerDto)
    {
        Account? existingAccount = await _unitOfWork.Accounts.GetByCondition(a => a.Email == registerDto.Email);
        if (existingAccount is not null)
        {
            throw new DuplicateEmailException(registerDto.Email);
        }
        Account newAccount = _mapper.Map<Account>(registerDto);

        if (registerDto.RoleName != "CLUB_MEMBER" && registerDto.RoleName != "CLUB_MANAGER")
            throw new NotFoundException($"Not support this role name {registerDto.RoleName} when register new user");
        Role? r = await _unitOfWork.Roles.GetByCondition(r => r.RoleName == registerDto.RoleName);
        if (r is null)
            throw new NotFoundException("Role not found.");
        newAccount.RoleID = r.RoleID;
        newAccount.Username = registerDto.FirstName + " " + registerDto.LastName;
        newAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
        newAccount.Status = AccountStatus.ACTIVE;
        newAccount.IsEmailVerified = false;

        // Tạo verification token
        string verificationToken = GenerateVerificationToken(newAccount);
        newAccount.VerificationToken = verificationToken;
        newAccount.VerificationTokenExpiryTime = DateTime.UtcNow.AddHours(24); // Token hết hạn sau 24 giờ

        await _unitOfWork.Accounts.Add(newAccount);
        await _unitOfWork.SaveChangeAsync();

        try
        {
            // Tạo verification URL từ appsettings
            string verificationUrl = $"{_appSettings.FrontendUrl}/verify-email?token={verificationToken}";

            //gửi mail xác thực email
            await _emailService.SendEmailVerificationAsync(
                newAccount.Email,
                newAccount.Username,
                verificationUrl,
                verificationToken);
        }
        catch (Exception emailEx)
        {
            _logger.LogError($"Failed to send email: {emailEx.Message}");
            throw new Exception("Failed to send verification email.", emailEx);
        }


        UserResponse user = _mapper.Map<UserResponse>(newAccount);
        //string accessToken = GenerateAccessToken(newAccount);
        //string refreshToken = GenerateRefreshToken(newAccount);
        return new AuthResponse
        {
            User = user
        };
    }
    public async Task<AuthResponse> AuthenticatedUser(LoginEmailDto request)
    {
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.Email == request.Email);
        if (account is null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        if (!account.IsEmailVerified)
            throw new UnauthorizedAccessException("Email is not verified.");
        if (account.Status != AccountStatus.ACTIVE)
            throw new UnauthorizedAccessException("Account is not active.");
        UserResponse user = _mapper.Map<UserResponse>(account);
        string accessToken = GenerateAccessToken(account);
        string refreshToken = GenerateRefreshToken(account);

        //Lưu refresh token & refresh token expiryTime vào db
        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        account.LastLogin = DateTime.UtcNow.AddHours(7);
        await _unitOfWork.SaveChangeAsync();
        //lưu vào redis

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = user
        };
    }

    public async Task<bool> Logout()
    {
        //var principal = GetPrincipalFromExpiredToken(accessToken);
        //string email = principal.Identity?.Name ?? throw new UnauthorizedAccessException("Invalid access token.");

        string email = _currentUserService.Email ?? throw new UnauthorizedAccessException("User not authenticated.");
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.Email == email);
        if (account is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        account.RefreshToken = null;
        account.RefreshTokenExpiryTime = null;

        //thêm access và refresh token vào blacklist trong redis...

        return await _unitOfWork.SaveChangeAsync() > 0;

    }
    private string GenerateAccessToken(Account account)
    {
        //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key) // From .env
        );
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserID", account.UserID.ToString()),
            new Claim(ClaimTypes.Name, account.Email),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role.RoleName),
            new Claim("TokenType", "AccessToken"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private string GenerateRefreshToken(Account account)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim("TokenType", "RefreshToken"),
            new Claim(("UserID"), account.UserID.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key)
            ),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;
        ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }
        return principal;
    }

    public async Task<UserResponse?> GetCurrentUserInfo()
    {
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.UserID == _currentUserService.UserId);
        if (account is null)
        {
            throw new UnauthorizedAccessException("Chưa xác thực. Lấy thông tin người dùng thất bại.");
        }

        IEnumerable<UserLevelResponseDto>? userLevelMax = null;
        IEnumerable<UserLevelResponseDto>? userLevel = null;
        try
        {
            userLevel = await _academyMicroserviceClient.GetUserLevelsAsync(account.UserID);
            userLevelMax = await _academyMicroserviceClient.GetUserLevelMaxAsync(account.UserID);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to get user level from Academy service for user {account.UserID}: {ex.Message}");
            // Continue without level if Academy service is unavailable
        }

        UserResponse userResponse = _mapper.Map<UserResponse>(account);
        return userResponse with
        {
            UserLevel = userLevel,
            UserLevelMax = userLevelMax
        };
    }

    public async Task<bool> UpdateProfileAsync(ProfileUpdateDto userUpdateDto)
    {
        try
        {
            Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.UserID == _currentUserService.UserId);
            if (account is null)
            {
                throw new UnauthorizedAccessException("Chưa xác thực. Cập nhật thông tin người dùng thất bại.");
            }
            account.Username = userUpdateDto.Username;
            account.UserInfo.FirstName = userUpdateDto.FirstName;
            account.UserInfo.LastName = userUpdateDto.LastName;
            account.UserInfo.DateOfBirth = userUpdateDto.DateOfBirth;
            account.UserInfo.Gender = userUpdateDto.Gender;
            account.UserInfo.Phone = userUpdateDto.Phone;
            Account? updatedAccount = await _unitOfWork.Accounts.Update(account);
            UserInfo? updatedUserInfo = await _unitOfWork.UserInfos.Update(account.UserInfo);
            await _unitOfWork.SaveChangeAsync();
        }
        catch (Exception)
        {
            throw new Exception("Cập nhật thông tin người dùng thất bại.");
        }


        return true;
    }

    public async Task<UserResponse?> VerifyEmailAsync(string token)
    {
        // Validate token
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Invalid verification token.");
        }

        // Tìm account có verification token
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.VerificationToken == token);
        if (account is null)
        {
            throw new UnauthorizedAccessException("Invalid verification token.");
        }

        // Kiểm tra xem token có hết hạn không
        if (account.VerificationTokenExpiryTime < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Verification token has expired. Please request a new one.");
        }

        // Cập nhật account
        account.Status = AccountStatus.ACTIVE;
        account.IsEmailVerified = true;
        account.VerificationToken = null;
        account.VerificationTokenExpiryTime = null;

        await _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangeAsync();

        // Tạo và gửi notification ngay khi register thành công (Real-time)
        try
        {
            await _notificationService.SendAndCreateNotificationAsync(
                account.UserID,
                "Đăng ký thành công",
                $"Chào mừng {account.Username}! Bạn đã đăng ký thành công vào Droniverse.",
                Domain.Enums.NotificationType.EMAIL,
                account.Email
            );
        }
        catch (Exception notificationEx)
        {
            _logger.LogError($"Failed to send notification: {notificationEx.Message}");
        }

        UserResponse userResponse = _mapper.Map<UserResponse>(account);
        return userResponse;
    }

    private string GenerateVerificationToken(Account account)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim("TokenType", "VerificationToken"),
            new Claim(("UserID"), account.UserID.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24), // Verification token hết hạn sau 24 giờ
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate service-to-service JWT token cho internal communication giữa microservices
    /// Token này không liên kết với user cụ thể, dùng cho giao tiếp giữa các service
    /// </summary>
    public string GenerateServiceToken(string serviceId)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key)
        );
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("ServiceID", serviceId),
            new Claim("TokenType", "ServiceToken"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Service token hết hạn sau 1 giờ
        var expirationTime = DateTime.UtcNow.AddHours(1);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expirationTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

