using AutoMapper;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;

namespace Droniverse.Identity.Application.Services;
internal class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoleResponse> AddRole(RoleCreateDto roleCreateDto)
    {
        Role role = new Role();
        role.RoleName = roleCreateDto.roleName;
        role.Description = roleCreateDto.description;

        Role addedRole = await _unitOfWork.Roles.Add(role);
        await _unitOfWork.SaveChangeAsync();
        RoleResponse response = _mapper.Map<RoleResponse>(addedRole);
        return response;
    }

    public async Task<RoleResponse> GetRoleById(Guid roleId)
    {
        Role? role = await _unitOfWork.Roles.GetByCondition(r => r.RoleID == roleId);
        if(role == null)
        {
            throw new Exception("Role not found");
        }
        RoleResponse response = _mapper.Map<RoleResponse>(role);
        return response;
    }

    public async Task<IEnumerable<RoleResponse>> GetAllRoles()
    {
        IEnumerable<Role> roles = await _unitOfWork.Roles.GetAll();
        IEnumerable<RoleResponse> response = _mapper.Map<IEnumerable<RoleResponse>>(roles);
        return response;
    }

    public async Task<RoleResponse> UpdateRole(Guid roleId, RoleUpdateDto roleUpdateDto)
    {
        Role? existingRole = await _unitOfWork.Roles.GetByCondition(r => r.RoleID == roleId);
        if (existingRole == null)
        {
            throw new Exception("Role not found");
        }

        _mapper.Map(roleUpdateDto, existingRole);
        await _unitOfWork.Roles.Update(existingRole);
        await _unitOfWork.SaveChangeAsync();
        RoleResponse response = _mapper.Map<RoleResponse>(existingRole);
        return response;
    }

    public async Task<bool> DeleteRole(Guid id)
    {
        Role? r = await _unitOfWork.Roles.GetByCondition(p => p.RoleID == id);
        if (r == null)
        {
            throw new Exception($"ROle with id \"{id}\" does not exist.");
        }

        await _unitOfWork.Roles.Delete(r);
        await _unitOfWork.SaveChangeAsync();
        return true;
    }
}

