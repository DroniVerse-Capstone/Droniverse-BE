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

        CreateMap<QuizQuestion, QuizQuestion>()
            .ForMember(dest => dest.QuestionID, opt => opt.Ignore())
            .ForMember(dest => dest.QuizID, opt => opt.Ignore())
            .ForMember(dest => dest.Quiz, opt => opt.Ignore())
            .ForMember(dest => dest.QuizQuestionAttempts, opt => opt.Ignore());

        CreateMap<QuizQuestion, QuizQuestionClientViewDTO>();

        CreateMap<QuizQuestion, QuizQuestionLearningDTO>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => BuildLearningOptions(src)));
        
        CreateMap<ImportQuizQuestionDTO, QuizQuestion>()
            .ForMember(dest => dest.QuestionID, opt => opt.Ignore())
            .ForMember(dest => dest.Quiz, opt => opt.Ignore())
            .ForMember(dest => dest.QuizQuestionAttempts, opt => opt.Ignore());
    }

    private static IReadOnlyList<QuizQuestionOptionLearningDTO> BuildLearningOptions(QuizQuestion question)
    {
        var options = new List<QuizQuestionOptionLearningDTO>
        {
            new() { OptionKey = "A", ContentVN = question.AnswerA, ContentEN = question.AnswerA_EN },
            new() { OptionKey = "B", ContentVN = question.AnswerB, ContentEN = question.AnswerB_EN },
            new() { OptionKey = "C", ContentVN = question.AnswerC, ContentEN = question.AnswerC_EN },
            new() { OptionKey = "D", ContentVN = question.AnswerD, ContentEN = question.AnswerD_EN }
        };

        for (var i = options.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (options[i], options[j]) = (options[j], options[i]);
        }

        return options;
    }
}
