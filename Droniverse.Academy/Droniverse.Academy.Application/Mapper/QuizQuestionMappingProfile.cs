using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class QuizQuestionMappingProfile : Profile
{
    public QuizQuestionMappingProfile()
    {
        CreateMap<CreateQuizQuestionRequestDTO, QuizQuestion>()
            .ForMember(dest => dest.QuestionID, opt => opt.Ignore())
            .ForMember(dest => dest.Quiz, opt => opt.Ignore())
            .ForMember(dest => dest.QuizQuestionAttempts, opt => opt.Ignore());

        CreateMap<UpdateQuizQuestionRequestDTO, QuizQuestion>()
            .ForMember(dest => dest.QuestionID, opt => opt.Ignore())
            .ForMember(dest => dest.QuizID, opt => opt.Ignore())
            .ForMember(dest => dest.Quiz, opt => opt.Ignore())
            .ForMember(dest => dest.QuizQuestionAttempts, opt => opt.Ignore());

        CreateMap<QuizQuestion, QuizQuestionClientViewDTO>();
    }
}
