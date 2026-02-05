using AutoMapper;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Identity.Application.Services;

internal class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public AuthService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = user
        };
    }

    private string GenerateAccessToken(Account account)
    {
        // Implementation for generating access token
        return "generated_access_token";
    }

    private string GenerateRefreshToken()
    {
        // Implementation for generating access token
        return "generated_refresh_token";
    }
}

