using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IFeedbackService
{
    Task<IEnumerable<FeedbackResponseDto>> GetAllFeedbacks();
    Task<FeedbackResponseDto> GetFeedbackById(Guid id);
    Task<FeedbackResponseDto> CreateFeedback(FeedbackCreateDto feedbackCreateDto);
    //    Task<FeedbackResponseDto> UpdateFeedback(Guid id, FeedbackUpdateDto feedbackUpdateDto);
}

