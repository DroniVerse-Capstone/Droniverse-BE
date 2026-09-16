using AutoMapper;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Application.Services;

internal class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PermissionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginationResult<IEnumerable<PermissionResponse>>> GetAllPermissions(
        PermissionSearchRequest permissionSearchRequest,
        int pageIndex,
        int pageSize)
    {
        permissionSearchRequest ??= new PermissionSearchRequest();
        
        return await _unitOfWork.Permissions.GetAllPermissionsAsync(
            permissionSearchRequest,
            permissionSearchRequest.CurrentPage,
            permissionSearchRequest.PageSize);
    }

    public async Task<PermissionResponse> GetPermissionById(Guid id)
    {
        Permission? permission = await _unitOfWork.Permissions.GetByCondition(p => p.PermissionID == id);
        PermissionResponse response = _mapper.Map<PermissionResponse>(permission);
        return response;
    }

    public async Task<PermissionResponse> AddPermission(PermissionCreateDto permissionCreateDto)
    {
        Permission? existingPermission = await _unitOfWork.Permissions.GetByCondition(p => p.PermissionName == permissionCreateDto.PermissionName);
        if (existingPermission != null)
        {
            throw new Exception($"Permission with the same name \"{permissionCreateDto.PermissionName}\" already exists.");
        }

        Permission p = _mapper.Map<Permission>(permissionCreateDto);
        await _unitOfWork.Permissions.Add(p);
        await _unitOfWork.SaveChangeAsync();
        PermissionResponse response = _mapper.Map<PermissionResponse>(p);
        return response;
    }

    public async Task<bool> DeletePermission(Guid id)
    {
        Permission? p = await _unitOfWork.Permissions.GetByCondition(p => p.PermissionID == id);
        if (p == null)
        {
            throw new Exception($"Permission with id \"{id}\" does not exist.");
        }

        await _unitOfWork.Permissions.Delete(p);
        await _unitOfWork.SaveChangeAsync();
        return true;
    }

    public async Task<PermissionResponse> UpdatePermission(Guid id, PermissionUpdateDto permissionUpdateDto)
    {
        Permission? existingPermission = await _unitOfWork.Permissions.GetByCondition(p => p.PermissionID == id);
        if (existingPermission == null)
        {
            throw new Exception($"Permission with id \"{id}\" does not exist.");
        }
        _mapper.Map(permissionUpdateDto, existingPermission);

        await _unitOfWork.Permissions.Update(existingPermission);
        await _unitOfWork.SaveChangeAsync();

        PermissionResponse response = _mapper.Map<PermissionResponse>(existingPermission);
        return response;
    }
}

