using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class LabLearningService : ILabLearningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILearningService _learningService;

    public LabLearningService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ILearningService learningService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _learningService = learningService;
    }

    public async Task<SubmitLabResultDTO> SubmitLabAsync(Guid enrollmentId, Guid labId, SubmitLabRequestDTO request)
    {
        ValidateRequest(request);

        var lab = await GetLabAsync(labId);
        var lesson = await GetLabLessonAsync(lab.LabID);

        await _learningService.ValidateLessonAccessAsync(enrollmentId, lesson.LessonID);

        var userLab = await UpsertUserLabAsync(labId, request);
        await _unitOfWork.SaveChangesAsync();

        var completion = await CompleteLessonIfNeededAsync(enrollmentId, lesson.LessonID, userLab.IsCompleted);
        return BuildSubmitResult(userLab, completion);
    }

    private static void ValidateRequest(SubmitLabRequestDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }

    private async Task<Lab> GetLabAsync(Guid labId)
    {
        return await _unitOfWork.Labs.GetByIdAsync(labId)
            ?? throw new NotFoundException("Không tìm thấy lab.");
    }

    private async Task<Lesson> GetLabLessonAsync(Guid labId)
    {
        return await _unitOfWork.Lessons.GetByConditionAsync(x => x.Type == LessonType.LAB && x.ReferenceID == labId)
            ?? throw new NotFoundException("Không tìm thấy lesson của lab.");
    }

    private async Task<UserLab> UpsertUserLabAsync(Guid labId, SubmitLabRequestDTO request)
    {
        var userLab = await _unitOfWork.UserLabs.GetByConditionAsync(
            x => x.UserID == _currentUser.UserId && x.LabID == labId);

        if (userLab == null)
        {
            userLab = BuildUserLab(labId, request);
            await _unitOfWork.UserLabs.AddAsync(userLab);
            return userLab;
        }

        MapRequestToUserLab(userLab, request);
        await _unitOfWork.UserLabs.UpdateAsync(userLab);
        return userLab;
    }

    private UserLab BuildUserLab(Guid labId, SubmitLabRequestDTO request)
    {
        return new UserLab
        {
            UserLabID = Guid.NewGuid(),
            UserID = _currentUser.UserId,
            LabID = labId,
            Solution = request.Solution,
            IsCompleted = request.IsCompleted,
            Time = request.Time,
            NumberOfStep = request.NumberOfStep,
            Length = request.Length,
            FeedbackVN = request.FeedbackVN,
            FeedbackEN = request.FeedbackEN,
            Rating = request.Rating,
            Point = request.Point
        };
    }

    private static void MapRequestToUserLab(UserLab userLab, SubmitLabRequestDTO request)
    {
        userLab.Solution = request.Solution;
        userLab.IsCompleted = request.IsCompleted;
        userLab.Time = request.Time;
        userLab.NumberOfStep = request.NumberOfStep;
        userLab.Length = request.Length;
        userLab.FeedbackVN = request.FeedbackVN;
        userLab.FeedbackEN = request.FeedbackEN;
        userLab.Rating = request.Rating;
        userLab.Point = request.Point;
    }

    private async Task<CompleteLessonResultDTO?> CompleteLessonIfNeededAsync(Guid enrollmentId, Guid lessonId, bool isCompleted)
    {
        if (!isCompleted)
            return null;

        return await _learningService.CompleteLessonAsync(enrollmentId, lessonId);
    }

    private static SubmitLabResultDTO BuildSubmitResult(UserLab userLab, CompleteLessonResultDTO? completion)
    {
        return new SubmitLabResultDTO
        {
            UserLabID = userLab.UserLabID,
            IsCompleted = userLab.IsCompleted,
            Completion = completion
        };
    }
}
