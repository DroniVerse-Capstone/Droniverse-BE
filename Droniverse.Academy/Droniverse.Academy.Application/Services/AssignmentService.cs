using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public AssignmentService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<AssignmentClientViewDTO> CreateAssignmentAsync(CreateAssignmentRequestDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TitleEN) || string.IsNullOrWhiteSpace(request.TitleVN))
            throw new ValidationException("Tiêu đề assignment là bắt buộc.");

        if (request.EstimatedTime < 0)
            throw new ValidationException("EstimatedTime phải lớn hơn hoặc bằng 0.");

        var assignment = new Assignment
        {
            AssignmentID = Guid.NewGuid(),
            TitleEN = request.TitleEN.Trim(),
            TitleVN = request.TitleVN.Trim(),
            DescriptionEN = request.DescriptionEN?.Trim() ?? string.Empty,
            DescriptionVN = request.DescriptionVN?.Trim() ?? string.Empty,
            Requirement = request.Requirement?.Trim() ?? string.Empty,
            EstimatedTime = request.EstimatedTime
        };

        assignment.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Assignments.AddAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        return new AssignmentClientViewDTO
        {
            AssignmentID = assignment.AssignmentID,
            TitleEN = assignment.TitleEN,
            TitleVN = assignment.TitleVN,
            DescriptionEN = assignment.DescriptionEN,
            DescriptionVN = assignment.DescriptionVN,
            Requirement = assignment.Requirement,
            EstimatedTime = assignment.EstimatedTime,
            CreateBy = assignment.CreateBy,
            UpdateBy = assignment.UpdateBy,
            CreateAt = assignment.CreateAt,
            UpdateAt = assignment.UpdateAt
        };
    }
}
