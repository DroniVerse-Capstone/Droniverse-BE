using AutoMapper;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Application.Mapper;
public class PermissionMappingProfile : Profile
{
    public PermissionMappingProfile()
    {
        CreateMap<PermissionCreateDto, Permission>()
            .ForMember(dest => dest.PermissionID, opt => opt.Ignore())
            .ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.PermissionName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<PermissionUpdateDto, Permission>()
            .ForMember(dest => dest.PermissionID, opt => opt.Ignore())
            .ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.PermissionName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<Permission, PermissionResponse>()
            .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.PermissionID))
            .ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.PermissionName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
    }
}
