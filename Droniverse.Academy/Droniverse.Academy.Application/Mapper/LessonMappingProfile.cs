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
            .ForMember(dest => dest.UserLessons, opt => opt.Ignore());

        CreateMap<UpdateLessonRequestDTO, Lesson>()
            .ForMember(dest => dest.LessonID, opt => opt.Ignore())
            .ForMember(dest => dest.ModuleID, opt => opt.Ignore())
            .ForMember(dest => dest.OrderIndex, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore())
            .ForMember(dest => dest.UserLessons, opt => opt.Ignore());

        CreateMap<Lesson, Lesson>()
            .ForMember(dest => dest.LessonID, opt => opt.Ignore())
            .ForMember(dest => dest.ModuleID, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore())
            .ForMember(dest => dest.UserLessons, opt => opt.Ignore());

        CreateMap<Lesson, LessonClientViewDTO>();

        CreateMap<WebSimulator, WebSimulator>();
        CreateMap<VRSimulator, VRSimulator>();

        CreateMap<Theory, LessonClientViewDTO>()
            .ForMember(dest => dest.TitleVN, opt => opt.MapFrom(src => src.TitleVN))
            .ForMember(dest => dest.TitleEN, opt => opt.MapFrom(src => src.TitleEN))
            .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.EstimatedTime));

        CreateMap<Quiz, LessonClientViewDTO>()
            .ForMember(dest => dest.TitleVN, opt => opt.MapFrom(src => src.TitleVN))
            .ForMember(dest => dest.TitleEN, opt => opt.MapFrom(src => src.TitleEN))
            .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.TimeLimit));

        CreateMap<Lab, LessonClientViewDTO>()
            .ForMember(dest => dest.TitleVN, opt => opt.MapFrom(src => src.NameVN))
            .ForMember(dest => dest.TitleEN, opt => opt.MapFrom(src => src.NameEN))
            .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.EstimatedTime))
            .ForMember(dest => dest.Type, opt => opt.Ignore());

        CreateMap<WebSimulator, LessonClientViewDTO>()
            .ForMember(dest => dest.TitleVN, opt => opt.MapFrom(src => src.TitleVN))
            .ForMember(dest => dest.TitleEN, opt => opt.MapFrom(src => src.TitleEN))
            .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.EstimatedTime))
            .ForMember(dest => dest.Type, opt => opt.Ignore());

        CreateMap<VRSimulator, LessonClientViewDTO>()
            .ForMember(dest => dest.TitleVN, opt => opt.MapFrom(src => src.TitleVN))
            .ForMember(dest => dest.TitleEN, opt => opt.MapFrom(src => src.TitleEN))
            .ForMember(dest => dest.EstimatedTime, opt => opt.MapFrom(src => src.EstimatedTime))
            .ForMember(dest => dest.Type, opt => opt.Ignore());
    }
}
