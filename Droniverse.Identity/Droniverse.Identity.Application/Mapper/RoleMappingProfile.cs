using AutoMapper;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Domain.Entities;

namespace Droniverse.Identity.Application.Mapper;
public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<Role, RoleResponse>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleID))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<RoleCreateDto, Role>()
            .ForMember(dest => dest.RoleID, opt => opt.Ignore() )
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.roleName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description));
    }
}

