using AutoMapper;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Droniverse.Identity.Application.Services;

internal class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
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
        UserResponse user = _mapper.Map<UserResponse>(account);
        string newAccessToken = GenerateAccessToken(account);
        string newRefreshToken = GenerateRefreshToken();
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
        newAccount.RoleID = (await _unitOfWork.Roles.GetByCondition(r => r.RoleName == "CLUB_MEMBER")).RoleID;
        await _unitOfWork.Accounts.Add(newAccount);
        await _unitOfWork.SaveChangeAsync();
        UserResponse user = _mapper.Map<UserResponse>(newAccount);
        string accessToken = GenerateAccessToken(newAccount);
        string refreshToken = GenerateRefreshToken();
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = user
        };
    }
    public async Task<AuthResponse> AuthenticatedUser(string email, string password)
    {
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.Email == email && a.PasswordHash == password);
        if(account is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        UserResponse user = _mapper.Map<UserResponse>(account);
        string accessToken = GenerateAccessToken(account);
        string refreshToken = GenerateRefreshToken();

        //Lưu refresh token & refresh token expiryTime vào db
        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
        
        //lưu vào redis

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = user
        };
    }

    public async Task<bool> Logout(string accessToken, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        string email = principal.Identity?.Name ?? throw new UnauthorizedAccessException("Invalid access token.");

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
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserID", account.UserID.ToString()),
            new Claim(ClaimTypes.Name, account.Email),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role.RoleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
            ValidateLifetime = false
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;
        ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }
        return principal;
    }
}

