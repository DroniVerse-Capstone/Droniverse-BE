using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

namespace Droniverse.Academy.Application.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public FeedbackService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<FeedbackClientViewDTO> CreateFeedbackForCourseVersionAsync(Guid courseId, Guid versionId, FeedbackCreateDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var courseVersion = await EnsureCourseVersionExistsAsync(courseId, versionId);

        ValidateRating(request.Rating);

        var feedback = _mapper.Map<Feedback>(request);
        feedback.FeedbackID = Guid.NewGuid();
        feedback.CourseVersionID = courseVersion.CourseVersionID;
        feedback.UserID = _currentUser.UserId;
        feedback.CreatedAt = _clock.Now;

        await _unitOfWork.Feedbacks.AddAsync(feedback);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Feedbacks.GetByConditionAsync(
            f => f.FeedbackID == feedback.FeedbackID,
            includeProperties: "CourseVersion,CourseVersion.CourseVersionCategories,CourseVersion.RequiredDrones");

        return _mapper.Map<FeedbackClientViewDTO>(created ?? feedback);
    }

    public async Task<IEnumerable<FeedbackClientViewDTO>> GetFeedbacksByCourseVersionAsync(Guid courseId, Guid versionId)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var feedbacks = await _unitOfWork.Feedbacks.GetAllAsync(
            filter: f => f.CourseVersionID == versionId,
            orderBy: q => q.OrderByDescending(x => x.CreatedAt),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "CourseVersion,CourseVersion.CourseVersionCategories,CourseVersion.RequiredDrones");

        return _mapper.Map<IEnumerable<FeedbackClientViewDTO>>(feedbacks.Data);
    }

    public async Task<FeedbackClientViewDTO> GetFeedbackDetailAsync(Guid courseId, Guid versionId, Guid feedbackId)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var feedback = await _unitOfWork.Feedbacks.GetByConditionAsync(
            f => f.FeedbackID == feedbackId && f.CourseVersionID == versionId,
            includeProperties: "CourseVersion,CourseVersion.CourseVersionCategories,CourseVersion.RequiredDrones");

        if (feedback == null)
            throw new BaseException("Không tìm thấy phản hồi.", "NOT_FOUND");

        return _mapper.Map<FeedbackClientViewDTO>(feedback);
    }

    public async Task<FeedbackClientViewDTO> UpdateFeedbackAsync(Guid courseId, Guid versionId, Guid feedbackId, FeedbackUpdateDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        await EnsureCourseVersionExistsAsync(courseId, versionId);
        ValidateRating(request.Rating);

        var feedback = await _unitOfWork.Feedbacks.GetByConditionAsync(
            f => f.FeedbackID == feedbackId && f.CourseVersionID == versionId,
            includeProperties: "CourseVersion,CourseVersion.CourseVersionCategories,CourseVersion.RequiredDrones");

        if (feedback == null)
            throw new BaseException("Không tìm thấy phản hồi.", "NOT_FOUND");

        if (feedback.UserID != _currentUser.UserId)
            throw new ForbiddenException("Bạn chỉ có thể cập nhật phản hồi của chính mình.");

        _mapper.Map(request, feedback);

        await _unitOfWork.Feedbacks.UpdateAsync(feedback);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<FeedbackClientViewDTO>(feedback);
    }

    public async Task DeleteFeedbackAsync(Guid courseId, Guid versionId, Guid feedbackId)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var feedback = await _unitOfWork.Feedbacks.GetByConditionAsync(
            f => f.FeedbackID == feedbackId && f.CourseVersionID == versionId);

        if (feedback == null)
            throw new BaseException("Không tìm thấy phản hồi.", "NOT_FOUND");

        await _unitOfWork.Feedbacks.DeleteAsync(feedback);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<CourseVersion> EnsureCourseVersionExistsAsync(Guid courseId, Guid versionId)
    {
        var courseVersion = await _unitOfWork.CourseVersions.GetByConditionAsync(
            cv => cv.CourseVersionID == versionId && cv.CourseID == courseId);

        if (courseVersion == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        return courseVersion;
    }

    private static void ValidateRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ValidationException("Điểm đánh giá phải từ 1 đến 5.");
    }
}
