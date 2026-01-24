using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;

namespace Droniverse.Academy.Application.Services;

internal class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<FeedbackResponseDto> CreateFeedback(FeedbackCreateDto feedbackCreateDto)
    {
        if(feedbackCreateDto == null) {
            throw new ArgumentNullException(nameof(feedbackCreateDto));
        }
        Feedback feedback = _mapper.Map<Feedback>(feedbackCreateDto);
        feedback.FeedbackID = Guid.NewGuid();
        //feedback.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.Feedbacks.Add(feedback);
        await _unitOfWork.SaveChangesAsync();
        FeedbackResponseDto response = _mapper.Map<FeedbackResponseDto>(feedback);
        return response;
    }

    public async Task<IEnumerable<FeedbackResponseDto>> GetAllFeedbacks()
    {
        IEnumerable<Feedback> feedbackList = await _unitOfWork.Feedbacks.GetAll();
        IEnumerable<FeedbackResponseDto> responses = _mapper.Map<IEnumerable<FeedbackResponseDto>>(feedbackList);
        return responses;
    }

    public async Task<FeedbackResponseDto> GetFeedbackById(Guid id)
    {
        Feedback? feedback = await _unitOfWork.Feedbacks.GetByCondition(f => f.FeedbackID == id);
        if (feedback == null)
        {
            throw new KeyNotFoundException($"Feedback with ID {id} not found.");
        }
        FeedbackResponseDto response = _mapper.Map<FeedbackResponseDto>(feedback);
        return response;
    }
}
