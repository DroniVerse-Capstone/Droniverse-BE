using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class LessonMappingProfile : Profile
{
    public LessonMappingProfile()
    {
        CreateMap<CreateLessonRequestDTO, Lesson>()
            .ForMember(dest => dest.LessonID, opt => opt.Ignore())
            .ForMember(dest => dest.ModuleID, opt => opt.Ignore())
            .ForMember(dest => dest.OrderIndex, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore())
            .ForMember(dest => dest.UserLessons, opt => opt.Ignore())
            .ForMember(dest => dest.Lab, opt => opt.Ignore())
            .ForMember(dest => dest.Theory, opt => opt.Ignore())
            .ForMember(dest => dest.Quiz, opt => opt.Ignore());

        CreateMap<UpdateLessonRequestDTO, Lesson>()
            .ForMember(dest => dest.LessonID, opt => opt.Ignore())
            .ForMember(dest => dest.ModuleID, opt => opt.Ignore())
            .ForMember(dest => dest.OrderIndex, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore())
            .ForMember(dest => dest.UserLessons, opt => opt.Ignore())
            .ForMember(dest => dest.Lab, opt => opt.Ignore())
            .ForMember(dest => dest.Theory, opt => opt.Ignore())
            .ForMember(dest => dest.Quiz, opt => opt.Ignore());

        CreateMap<Lesson, LessonClientViewDTO>();
    }
}
