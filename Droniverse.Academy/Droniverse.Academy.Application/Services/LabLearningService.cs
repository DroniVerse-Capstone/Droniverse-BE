using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.IService.Mongo;
using AutoMapper;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class LabLearningService : ILabLearningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILearningService _learningService;
    private readonly LearningAssessmentAccessService _assessmentAccessService;
    private readonly ILabContentService _labContentService;
    private readonly IMapper _mapper;

    public LabLearningService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILearningService learningService,
        LearningAssessmentAccessService assessmentAccessService,
        ILabContentService labContentService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _learningService = learningService;
        _assessmentAccessService = assessmentAccessService;
        _labContentService = labContentService;
        _mapper = mapper;
    }

    public async Task<LabLearningStateDTO> GetLabLearningStateAsync(Guid enrollmentId, Guid labId)
    {
        var (lab, _) = await _assessmentAccessService.GetAccessibleLabAsync(enrollmentId, labId);
        var labContent = await _labContentService.GetByLabIdAsync(labId)
            ?? await _labContentService.CreateEmptyAsync(labId);

        var userLab = await _unitOfWork.UserLabs.GetByConditionAsync(
            x => x.UserID == _currentUser.UserId && x.LabID == labId);

        return new LabLearningStateDTO
        {
            Lab = _mapper.Map<LabClientViewDTO>(lab),
            LabContent = labContent,
            UserLab = userLab == null ? null : _mapper.Map<UserLabResponseDTO>(userLab)
        };
    }

    public async Task<LabLearningMiniDTO> GetLabLearningMiniAsync(Guid enrollmentId, Guid labId)
    {
        var (lab, _) = await _assessmentAccessService.GetAccessibleLabAsync(enrollmentId, labId);

        var userLab = await _unitOfWork.UserLabs.GetByConditionAsync(
            x => x.UserID == _currentUser.UserId && x.LabID == labId);

        return new LabLearningMiniDTO
        {
            Lab = _mapper.Map<LabClientViewDTO>(lab),
            UserLab = userLab == null ? null : _mapper.Map<UserLabResponseDTO>(userLab)
        };
    }

    public async Task<SubmitLabResultDTO> SubmitLabAsync(Guid enrollmentId, Guid labId, SubmitLabRequestDTO request)
    {
        ValidateRequest(request);

        var (_, lesson) = await _assessmentAccessService.GetAccessibleLabAsync(enrollmentId, labId);

        var userLab = await UpsertUserLabAsync(labId, request);
        await _unitOfWork.SaveChangesAsync();

        var completion = await CompleteLessonIfNeededAsync(enrollmentId, lesson.LessonID, userLab.IsCompleted);
        return BuildSubmitResult(userLab, completion);
    }

    private static void ValidateRequest(SubmitLabRequestDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);
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

        _mapper.Map(request, userLab);
        await _unitOfWork.UserLabs.UpdateAsync(userLab);
        return userLab;
    }

    private UserLab BuildUserLab(Guid labId, SubmitLabRequestDTO request)
    {
        var userLab = _mapper.Map<UserLab>(request);
        userLab.UserLabID = Guid.NewGuid();
        userLab.UserID = _currentUser.UserId;
        userLab.LabID = labId;
        return userLab;
    }

    private async Task<CompleteLessonResultDTO?> CompleteLessonIfNeededAsync(Guid enrollmentId, Guid lessonId, bool isCompleted)
    {
        if (!isCompleted)
            return null;

        return await _learningService.CompleteLessonByAssessmentAsync(enrollmentId, lessonId);
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
