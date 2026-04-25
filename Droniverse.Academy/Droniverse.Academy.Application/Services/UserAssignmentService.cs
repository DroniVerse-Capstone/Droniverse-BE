using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class UserAssignmentService : IUserAssignmentService
{
    private const int PassScoreThreshold = 80;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly LearningAssessmentAccessService _assessmentAccessService;
    private readonly CommunityMicroserviceClient _communityClient;

    public UserAssignmentService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IClock clock,
        LearningAssessmentAccessService assessmentAccessService,
        CommunityMicroserviceClient communityClient)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _assessmentAccessService = assessmentAccessService;
        _communityClient = communityClient;
    }

    public async Task<UserAssignmentSubmitResponseDTO> SubmitAssignmentAsync(Guid enrollmentId, Guid assignmentId, SubmitUserAssignmentRequestDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.MediaID == Guid.Empty)
            throw new ValidationException("MediaID là bắt buộc.");

        await _assessmentAccessService.GetAccessibleAssignmentAsync(enrollmentId, assignmentId);

        var latestAttemptResult = await _unitOfWork.UserAssignments.GetAllAsync(
            filter: x => x.AssignmentID == assignmentId && x.EnrollmentID == enrollmentId,
            orderBy: q => q.OrderByDescending(x => x.AttemptNumber),
            pageIndex: 1,
            pageSize: 1);

        var nextAttemptNumber = (latestAttemptResult.Data.FirstOrDefault()?.AttemptNumber ?? 0) + 1;

        var userAssignment = new UserAssignment
        {
            UserAssignmentID = Guid.NewGuid(),
            AssignmentID = assignmentId,
            EnrollmentID = enrollmentId,
            AttemptNumber = nextAttemptNumber,
            MediaID = request.MediaID,
            Description = request.Description?.Trim() ?? string.Empty,
            Status = UserAssignmentStatus.SUBMITTED,
            SubmittedAt = _clock.Now
        };

        await _unitOfWork.UserAssignments.AddAsync(userAssignment);
        await _unitOfWork.SaveChangesAsync();

        return new UserAssignmentSubmitResponseDTO
        {
            UserAssignmentID = userAssignment.UserAssignmentID,
            AssignmentID = userAssignment.AssignmentID,
            EnrollmentID = userAssignment.EnrollmentID,
            AttemptNumber = userAssignment.AttemptNumber,
            Status = userAssignment.Status,
            SubmittedAt = userAssignment.SubmittedAt
        };
    }

    public async Task<UserAssignmentReviewResponseDTO> ReviewAssignmentAsync(Guid userAssignmentId, ReviewUserAssignmentRequestDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Score is < 0 or > 100)
            throw new ValidationException("Score phải nằm trong khoảng từ 0 đến 100.");

        var userAssignment = await _unitOfWork.UserAssignments.GetByConditionAsync(
            x => x.UserAssignmentID == userAssignmentId,
            includeProperties: "Enrollment") ?? throw new NotFoundException("Không tìm thấy bài nộp assignment.");

        if (userAssignment.Status is UserAssignmentStatus.PASSED or UserAssignmentStatus.FAILED)
            throw new ValidationException("Bài nộp đã được chấm, không thể chấm lại.");

        var enrollment = userAssignment.Enrollment
            ?? throw new NotFoundException("Không tìm thấy enrollment của bài nộp.");

        var canAccessClub = await _communityClient.CheckParticipantByClubAsync(
            enrollment.ClubID,
            _currentUser.UserId,
            ParticipationStatus.ACTIVE);

        if (!canAccessClub)
            throw new ForbiddenException("Bạn không có quyền chấm assignment của club này.");

        var reviewedAt = _clock.Now;
        var isPassed = request.Score >= PassScoreThreshold;

        userAssignment.Score = request.Score;
        userAssignment.ReviewComment = request.ReviewComment?.Trim();
        userAssignment.ReviewedBy = _currentUser.UserId;
        userAssignment.ReviewedAt = reviewedAt;
        userAssignment.Status = isPassed ? UserAssignmentStatus.PASSED : UserAssignmentStatus.FAILED;

        await _unitOfWork.UserAssignments.UpdateAsync(userAssignment);
        await _unitOfWork.SaveChangesAsync();

        return new UserAssignmentReviewResponseDTO
        {
            UserAssignmentID = userAssignment.UserAssignmentID,
            AssignmentID = userAssignment.AssignmentID,
            EnrollmentID = userAssignment.EnrollmentID,
            Score = request.Score,
            IsPassed = isPassed,
            Status = userAssignment.Status,
            ReviewComment = userAssignment.ReviewComment,
            ReviewedBy = userAssignment.ReviewedBy ?? Guid.Empty,
            ReviewedAt = reviewedAt
        };
    }
}
