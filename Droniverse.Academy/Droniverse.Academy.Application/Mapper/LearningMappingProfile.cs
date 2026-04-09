using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class LearningMappingProfile : Profile
{
    public LearningMappingProfile()
    {
        CreateMap<Enrollment, LearningPathDTO>()
            .ForMember(dest => dest.Modules, opt => opt.Ignore());

        CreateMap<Module, LearningPathModuleDTO>()
            .ForMember(dest => dest.Progress, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore())
            .ForMember(dest => dest.Lessons, opt => opt.Ignore());

        CreateMap<Lesson, LearningPathLessonDTO>()
            .ForMember(dest => dest.Progress, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore())
            .ForMember(dest => dest.LastAccessDate, opt => opt.Ignore());
    }
}
