using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class UserLabMappingProfile : Profile
{
    public UserLabMappingProfile()
    {
        CreateMap<SubmitLabRequestDTO, UserLab>()
            .ForMember(dest => dest.UserLabID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.LabID, opt => opt.Ignore())
            .ForMember(dest => dest.Lab, opt => opt.Ignore());

        CreateMap<CreateUserLabRequestDTO, UserLab>()
            .ForMember(dest => dest.UserLabID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.Lab, opt => opt.Ignore());

        CreateMap<UserLab, UserLabResponseDTO>();
    }
}
