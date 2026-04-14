using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class UserQuizQuestionAttemptMappingProfile : Profile
{
    public UserQuizQuestionAttemptMappingProfile()
    {
        CreateMap<CreateUserQuizQuestionAttemptRequestDTO, QuizQuestionAttempt>()
            .ForMember(dest => dest.AttemptAnswerID, opt => opt.Ignore())
            .ForMember(dest => dest.QuizAttempt, opt => opt.Ignore())
            .ForMember(dest => dest.QuizQuestion, opt => opt.Ignore())
            .ForMember(dest => dest.IsCorrect, opt => opt.Ignore())
            .ForMember(dest => dest.Score, opt => opt.Ignore());

        CreateMap<QuizQuestionAttempt, UserQuizQuestionAttemptResponseDTO>();

        CreateMap<QuizQuestionAttempt, QuizQuestionAttemptReviewDTO>()
            .ForMember(dest => dest.Attempt, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Question, opt => opt.MapFrom(src => src.QuizQuestion));
    }
}
