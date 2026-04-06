using AutoMapper;
using Droniverse.Identity.Application.DTO.Extension;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Messages.User;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Identity.Application.Services;
internal class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserPublisher _publisher;
    private readonly ICloudinaryService _cloudinaryService;
    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IUserPublisher publisher,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _publisher = publisher;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsers()
    {
        IEnumerable<Account> accountList = await _unitOfWork.Accounts.GetAll();
        IEnumerable<UserResponse> userResponseList = _mapper.Map<IEnumerable<UserResponse>>(accountList);
        return userResponseList;
    }

    public async Task<UserResponse> AddUser(UserCreateDto userCreateDto)
    {
        Role? role = await _unitOfWork.Roles.GetByCondition(r => r.RoleID == userCreateDto.RoleId);
        if (role == null)
        {
            throw new ArgumentException("Invalid role");
        }
        Account newAccount = new Account
        {
            Username = userCreateDto.Username,
            PasswordHash = userCreateDto.PasswordHash,
            Email = userCreateDto.Email,
            CreateAt = DateTime.UtcNow,
            RoleID = userCreateDto.RoleId

        };
        Account createdAccount = await _unitOfWork.Accounts.Add(newAccount);
        UserInfo newUserInfo = new UserInfo
        {
            UserID = createdAccount.UserID,
            FirstName = userCreateDto.FirstName,
            LastName = userCreateDto.LastName,
            DateOfBirth = userCreateDto.DateOfBirth,

        };

        UserInfo createdUserInfo = await _unitOfWork.UserInfos.Add(newUserInfo);
        await _unitOfWork.SaveChangeAsync();

        UserResponse userResponse = _mapper.Map<UserResponse>(createdAccount);
        return userResponse;
    }

    public async Task<UserResponse> GetUserById(Guid id)
    {
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.UserID == id);
        if (account == null)
        {
            throw new ArgumentException($"Account id not found #{id}");
        }
        UserInfo? userInfo = await _unitOfWork.UserInfos.GetByCondition(a => a.UserID == id);
        if (userInfo == null)
        {
            throw new ArgumentException($"User info id not found #{id}");
        }

        UserResponse userResponse = _mapper.Map<UserResponse>(account);
        return userResponse;
    }

    public async Task<UserResponse> UpdateUser(Guid userId, UserUpdateDto userUpdateDto)
    {
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.UserID == userId);
        if (account == null)
        {
            throw new ArgumentException($"Account id not found #{userId}");
        }
        UserInfo? userInfo = await _unitOfWork.UserInfos.GetByCondition(a => a.UserID == userId);
        if (userInfo == null)
        {
            throw new ArgumentException($"User info id not found #{userId}");
        }

        //Update fields
        account.Username = userUpdateDto.Username;
        userInfo.FirstName = userUpdateDto.FirstName;
        userInfo.LastName = userUpdateDto.LastName;
        userInfo.DateOfBirth = userUpdateDto.DateOfBirth;

        Account? updatedAcc = await _unitOfWork.Accounts.Update(account);
        UserInfo? updatedUserInfo = await _unitOfWork.UserInfos.Update(userInfo);
        await _unitOfWork.SaveChangeAsync();
        //mapper
        UserResponse userResponse = _mapper.Map<UserResponse>(updatedAcc);
        return userResponse;
    }

    public async Task<bool> DeleteUser(Guid id)
    {
        try
        {
            Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.UserID == id);
            bool isDelete = await _unitOfWork.Accounts.Delete(account);
            await _unitOfWork.SaveChangeAsync();
            if (isDelete)
            {
                Dictionary<string, object> headers = new Dictionary<string, object>()
                {
                    {"event", "user.delete" },
                    {"rowCount", 1 }
                };
                UserDeletionMessage userDeletionMessage = new UserDeletionMessage(account.UserID, account.Username);

                //ktra redis

                _publisher.Publish<UserDeletionMessage>(headers, userDeletionMessage);
            }
            return isDelete;
        }
        catch (Exception)
        {
            return false;
            throw;
        }
    }

    public async Task<string> UploadUserAvatar(Guid userId, IFormFile imageFile)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<UserResponse>> GetUsersByIds(IEnumerable<Guid> userIds)
    {
        if (userIds == null || !userIds.Any())
            return [];

        return await _unitOfWork.Accounts.GetUsersByIdsAsync(userIds);
    }

    public async Task<IEnumerable<Guid>> GetUsersByUserInfo(UserInfoSearchRequest request)
    {
        var sortDirection = request?.SortDirection ?? SortDirection.Asc;
        var userIds = await _unitOfWork.Accounts.GetUsersByUserInfoAsync(
            request?.SearchName,
            sortDirection);

        return userIds;
    }
}
