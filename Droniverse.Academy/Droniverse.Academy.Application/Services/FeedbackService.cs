using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Abstractions;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public FeedbackService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUser currentUser,
        IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<FeedbackResponseDTO> CreateFeedback(FeedbackCreateDTO feedbackCreateDto)
    {
        if (feedbackCreateDto == null)
            throw new ArgumentNullException(nameof(feedbackCreateDto));

        var feedback = _mapper.Map<Feedback>(feedbackCreateDto);
        feedback.FeedbackID = Guid.NewGuid();
        feedback.UserID = _currentUser.UserId;
        feedback.CreatedAt = _clock.Now;

        await _unitOfWork.Feedbacks.AddAsync(feedback);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Feedbacks.GetByConditionAsync(
            f => f.FeedbackID == feedback.FeedbackID,
            includeProperties: "CourseVersion,CourseVersion.CourseVersionCategories,CourseVersion.RequiredDrones");

        return _mapper.Map<FeedbackResponseDTO>(created ?? feedback);
    }

    public async Task<IEnumerable<FeedbackResponseDTO>> GetAllFeedbacks()
    {
        var feedbacks = await _unitOfWork.Feedbacks.GetAllAsync(
            includeProperties: "CourseVersion,CourseVersion.CourseVersionCategories,CourseVersion.RequiredDrones");

        return _mapper.Map<IEnumerable<FeedbackResponseDTO>>(feedbacks.Data);
    }

    public async Task<FeedbackResponseDTO> GetFeedbackById(Guid id)
    {
        var feedback = await _unitOfWork.Feedbacks.GetByConditionAsync(
            f => f.FeedbackID == id,
            includeProperties: "CourseVersion,CourseVersion.CourseVersionCategories,CourseVersion.RequiredDrones");

        if (feedback == null)
            throw new BaseException($"Feedback with ID {id} not found.", "NOT_FOUND");

        return _mapper.Map<FeedbackResponseDTO>(feedback);
    }
}
