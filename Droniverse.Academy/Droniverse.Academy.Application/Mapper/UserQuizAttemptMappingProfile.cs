using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class UserQuizAttemptMappingProfile : Profile
{
    public UserQuizAttemptMappingProfile()
    {
        CreateMap<CreateUserQuizAttemptRequestDTO, QuizAttempt>()
            .ForMember(dest => dest.AttemptID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.StartTime, opt => opt.Ignore())
            .ForMember(dest => dest.SubmitTime, opt => opt.Ignore())
            .ForMember(dest => dest.Score, opt => opt.Ignore())
            .ForMember(dest => dest.IsPassed, opt => opt.Ignore())
            .ForMember(dest => dest.Quiz, opt => opt.Ignore())
            .ForMember(dest => dest.QuizQuestionAttempts, opt => opt.Ignore());

        CreateMap<QuizAttempt, UserQuizAttemptResponseDTO>();
        CreateMap<QuizAttempt, QuizAttemptDTO>();
    }
}
