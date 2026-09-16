using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class UserLessonMappingProfile : Profile
{
    public UserLessonMappingProfile()
    {
        CreateMap<CreateUserLessonRequestDTO, UserLesson>()
            .ForMember(dest => dest.UserLessonID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.LastAccessDate, opt => opt.Ignore())
            .ForMember(dest => dest.Lesson, opt => opt.Ignore());

        CreateMap<UserLesson, UserLessonResponseDTO>();
    }
}
