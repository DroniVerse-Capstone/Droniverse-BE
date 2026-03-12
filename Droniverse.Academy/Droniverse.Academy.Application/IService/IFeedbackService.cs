using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IFeedbackService
{
    Task<IEnumerable<FeedbackResponseDTO>> GetAllFeedbacks();
    Task<FeedbackResponseDTO> GetFeedbackById(Guid id);
    Task<FeedbackResponseDTO> CreateFeedback(FeedbackCreateDTO feedbackCreateDto);
    //    Task<FeedbackResponseDto> UpdateFeedback(Guid id, FeedbackUpdateDto feedbackUpdateDto);
}

