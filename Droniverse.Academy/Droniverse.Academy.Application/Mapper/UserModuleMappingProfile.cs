using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class UserModuleMappingProfile : Profile
{
    public UserModuleMappingProfile()
    {
        CreateMap<CreateUserModuleRequestDTO, UserModule>()
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.EnrollDate, opt => opt.Ignore())
            .ForMember(dest => dest.CompleteDate, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore());

        CreateMap<UserModule, UserModuleResponseDTO>();
    }
}
