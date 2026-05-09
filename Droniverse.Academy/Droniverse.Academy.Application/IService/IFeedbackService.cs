using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IFeedbackService
{
    Task<FeedbackClientViewDTO> CreateFeedbackForCourseVersionAsync(Guid courseId, Guid versionId, FeedbackCreateDTO request);
    Task<IEnumerable<FeedbackClientViewDTO>> GetFeedbacksByCourseVersionAsync(Guid courseId, Guid versionId);
    Task<IEnumerable<FeedbackClientViewDTO>> GetFeedbacksByCourseAsync(Guid courseId);
    Task<FeedbackClientViewDTO> GetFeedbackDetailAsync(Guid courseId, Guid versionId, Guid feedbackId);
    Task<FeedbackClientViewDTO> UpdateFeedbackAsync(Guid courseId, Guid versionId, Guid feedbackId, FeedbackUpdateDTO request);
    Task DeleteFeedbackAsync(Guid courseId, Guid versionId, Guid feedbackId);
}

