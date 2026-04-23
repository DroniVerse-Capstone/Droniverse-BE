using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class UserSimulatorMappingProfile : Profile
{
    public UserSimulatorMappingProfile()
    {
        CreateMap<SubmitSimulatorRequestDto, UserSimulator>()
            .ForMember(dest => dest.UserSimulatorID, opt => opt.Ignore())
            .ForMember(dest => dest.UserLessonID, opt => opt.Ignore())
            .ForMember(dest => dest.UserLesson, opt => opt.Ignore())
            .ForMember(dest => dest.IsSuccess, opt => opt.Ignore());

        CreateMap<UserSimulator, UserSimulatorResponseDTO>();
    }
}