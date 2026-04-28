using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Enums;
using Droniverse.Shared.DTOs.Response;
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

        var passedAttempt = await _unitOfWork.UserAssignments.GetByConditionAsync(
            x => x.AssignmentID == assignmentId
                 && x.EnrollmentID == enrollmentId
                 && x.Status == UserAssignmentStatus.PASSED);

        if (passedAttempt != null)
            throw new ValidationException("Assignment đã đạt, không thể nộp thêm.");

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

    public async Task<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>> GetSubmissionsForReviewAsync(
        Guid? assignmentId,
        Guid? enrollmentId,
        UserAssignmentStatus? status,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var queryResult = await _unitOfWork.UserAssignments.GetAllAsync(
            filter: x =>
                (!assignmentId.HasValue || x.AssignmentID == assignmentId.Value) &&
                (!enrollmentId.HasValue || x.EnrollmentID == enrollmentId.Value) &&
                (!status.HasValue || x.Status == status.Value),
            orderBy: q => q.OrderByDescending(x => x.SubmittedAt).ThenByDescending(x => x.AttemptNumber),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "Enrollment,Assignment");

        var allItems = queryResult.Data.ToList();
        var accessByClub = new Dictionary<Guid, bool>();
        var accessibleItems = new List<UserAssignment>(allItems.Count);

        foreach (var item in allItems)
        {
            var enrollment = item.Enrollment;
            if (enrollment == null)
                continue;

            if (!accessByClub.TryGetValue(enrollment.ClubID, out var canAccess))
            {
                canAccess = await _communityClient.CheckParticipantByClubAsync(
                    enrollment.ClubID,
                    _currentUser.UserId,
                    ParticipationStatus.ACTIVE);

                accessByClub[enrollment.ClubID] = canAccess;
            }

            if (canAccess)
                accessibleItems.Add(item);
        }

        var normalizedPageIndex = pageIndex < 1 ? 1 : pageIndex;
        var normalizedPageSize = pageSize < 1 ? 10 : pageSize;

        var paged = accessibleItems
            .Skip((normalizedPageIndex - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(MapToAttemptResponse)
            .ToList();

        return new PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>(
            paged,
            accessibleItems.Count,
            normalizedPageIndex,
            normalizedPageSize);
    }

    public async Task<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>> GetAssignmentAttemptsByCourseAndClubAsync(
        Guid? courseId,
        Guid? clubId,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var queryResult = await _unitOfWork.UserAssignments.GetAllAsync(
            filter: x =>
                (!courseId.HasValue || x.Enrollment.CourseID == courseId.Value) &&
                (!clubId.HasValue || x.Enrollment.ClubID == clubId.Value),
            orderBy: q => q.OrderByDescending(x => x.SubmittedAt).ThenByDescending(x => x.AttemptNumber),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "Enrollment");

        var allItems = queryResult.Data.ToList();
        var accessByClub = new Dictionary<Guid, bool>();
        var accessibleItems = new List<UserAssignment>(allItems.Count);

        foreach (var item in allItems)
        {
            var enrollment = item.Enrollment;
            if (enrollment == null)
                continue;

            if (!accessByClub.TryGetValue(enrollment.ClubID, out var canAccess))
            {
                canAccess = await _communityClient.CheckParticipantByClubAsync(
                    enrollment.ClubID,
                    _currentUser.UserId,
                    ParticipationStatus.ACTIVE);

                accessByClub[enrollment.ClubID] = canAccess;
            }

            if (canAccess)
                accessibleItems.Add(item);
        }

        var normalizedPageIndex = pageIndex < 1 ? 1 : pageIndex;
        var normalizedPageSize = pageSize < 1 ? 10 : pageSize;

        var paged = accessibleItems
            .Skip((normalizedPageIndex - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(MapToAttemptResponse)
            .ToList();

        return new PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>(
            paged,
            accessibleItems.Count,
            normalizedPageIndex,
            normalizedPageSize);
    }

    public async Task<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>> GetMyAssignmentAttemptsAsync(
        Guid enrollmentId,
        Guid assignmentId,
        int pageIndex = 1,
        int pageSize = 10)
    {
        await _assessmentAccessService.GetAccessibleAssignmentAsync(enrollmentId, assignmentId);

        var result = await _unitOfWork.UserAssignments.GetAllAsync(
            filter: x => x.EnrollmentID == enrollmentId && x.AssignmentID == assignmentId,
            orderBy: q => q.OrderByDescending(x => x.AttemptNumber),
            pageIndex: pageIndex,
            pageSize: pageSize);

        var mapped = result.Data
            .Select(MapToAttemptResponse)
            .ToList();

        return new PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>(
            mapped,
            result.TotalRecords,
            result.PageIndex,
            result.PageSize);
    }

    private static UserAssignmentAttemptResponseDTO MapToAttemptResponse(UserAssignment entity)
    {
        return new UserAssignmentAttemptResponseDTO
        {
            UserAssignmentID = entity.UserAssignmentID,
            AssignmentID = entity.AssignmentID,
            EnrollmentID = entity.EnrollmentID,
            AttemptNumber = entity.AttemptNumber,
            MediaID = entity.MediaID,
            Description = entity.Description,
            Status = entity.Status,
            Score = entity.Score,
            ReviewComment = entity.ReviewComment,
            ReviewedBy = entity.ReviewedBy,
            ReviewedAt = entity.ReviewedAt,
            SubmittedAt = entity.SubmittedAt
        };
    }
}
