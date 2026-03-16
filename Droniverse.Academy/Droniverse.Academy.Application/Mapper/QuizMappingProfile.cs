using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class QuizMappingProfile : Profile
{
    public QuizMappingProfile()
    {
        CreateMap<CreateQuizRequestDTO, Quiz>()
            .ForMember(dest => dest.QuizID, opt => opt.Ignore())
            .ForMember(dest => dest.Lesson, opt => opt.Ignore())
            .ForMember(dest => dest.QuizQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore());

        CreateMap<UpdateQuizRequestDTO, Quiz>()
            .ForMember(dest => dest.QuizID, opt => opt.Ignore())
            .ForMember(dest => dest.LessonID, opt => opt.Ignore())
            .ForMember(dest => dest.Lesson, opt => opt.Ignore())
            .ForMember(dest => dest.QuizQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore());

        CreateMap<Quiz, QuizClientViewDTO>();
    }
}
