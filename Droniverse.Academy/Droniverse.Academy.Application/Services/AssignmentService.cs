using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Application.Helpers;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.DTOs.Response;
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

        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleID);
        if (module == null)
            throw new BaseException("Không tìm thấy mô-đun.", "NOT_FOUND");

        await CourseVersionDraftGuard.EnsureDraftByModuleIdAsync(
            _unitOfWork,
            request.ModuleID,
            "Chỉ được chỉnh sửa lesson assignment khi phiên bản khóa học ở trạng thái Draft.");

        var orderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(request.ModuleID);
        await ValidateOrderIndexAsync(request.ModuleID, orderIndex);

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

        var lesson = new Lesson
        {
            LessonID = Guid.NewGuid(),
            ModuleID = request.ModuleID,
            OrderIndex = orderIndex,
            Type = LessonType.ASSIGNMENT,
            ReferenceID = assignment.AssignmentID
        };

        await _unitOfWork.Lessons.AddAsync(lesson);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(assignment);
    }

    public async Task<PaginationResult<IEnumerable<AssignmentClientViewDTO>>> GetAssignmentsAsync(int pageIndex = 1, int pageSize = 10)
    {
        var result = await _unitOfWork.Assignments.GetAllAsync(
            orderBy: q => q.OrderByDescending(x => x.CreateAt).ThenBy(x => x.TitleEN),
            pageIndex: pageIndex,
            pageSize: pageSize);

        var items = result.Data.Select(MapToDto).ToList();
        return new PaginationResult<IEnumerable<AssignmentClientViewDTO>>(items, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<AssignmentClientViewDTO> GetAssignmentByIdAsync(Guid assignmentId)
    {
        var assignment = await GetAssignmentEntityAsync(assignmentId);
        return MapToDto(assignment);
    }

    public async Task<AssignmentClientViewDTO> UpdateAssignmentAsync(Guid assignmentId, UpdateAssignmentRequestDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TitleEN) || string.IsNullOrWhiteSpace(request.TitleVN))
            throw new ValidationException("Tiêu đề assignment là bắt buộc.");

        if (request.EstimatedTime < 0)
            throw new ValidationException("EstimatedTime phải lớn hơn hoặc bằng 0.");

        var assignment = await GetAssignmentEntityAsync(assignmentId);

        assignment.TitleEN = request.TitleEN.Trim();
        assignment.TitleVN = request.TitleVN.Trim();
        assignment.DescriptionEN = request.DescriptionEN?.Trim() ?? string.Empty;
        assignment.DescriptionVN = request.DescriptionVN?.Trim() ?? string.Empty;
        assignment.Requirement = request.Requirement?.Trim() ?? string.Empty;
        assignment.EstimatedTime = request.EstimatedTime;
        assignment.SetAuditOnUpdate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Assignments.UpdateAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(assignment);
    }

    public async Task DeleteAssignmentAsync(Guid assignmentId)
    {
        var assignment = await GetAssignmentEntityAsync(assignmentId);

        var lessonUsingAssignment = await _unitOfWork.Lessons.GetByConditionAsync(
            x => x.Type == LessonType.ASSIGNMENT && x.ReferenceID == assignmentId);

        if (lessonUsingAssignment != null)
            throw new ValidationException("Không thể xóa assignment đang được sử dụng trong lesson.");

        var hasSubmissions = await _unitOfWork.UserAssignments.GetByConditionAsync(x => x.AssignmentID == assignmentId);
        if (hasSubmissions != null)
            throw new ValidationException("Không thể xóa assignment đã có submission.");

        await _unitOfWork.Assignments.DeleteAsync(assignment);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Assignment> GetAssignmentEntityAsync(Guid assignmentId)
    {
        var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
        if (assignment == null)
            throw new NotFoundException("Không tìm thấy assignment.");

        return assignment;
    }

    private static AssignmentClientViewDTO MapToDto(Assignment assignment)
    {
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

    private async Task<int> GetNextOrderIndexAsync(Guid moduleId)
    {
        var lessons = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.ModuleID == moduleId,
            orderBy: q => q.OrderByDescending(l => l.OrderIndex),
            pageIndex: 1,
            pageSize: 1);

        var latest = lessons.Data.FirstOrDefault();
        return (latest?.OrderIndex ?? 0) + 1;
    }

    private async Task ValidateOrderIndexAsync(Guid moduleId, int orderIndex)
    {
        if (orderIndex <= 0)
            throw new ValidationException("OrderIndex phải lớn hơn 0.");

        var duplicated = await _unitOfWork.Lessons.GetByConditionAsync(
            l => l.ModuleID == moduleId && l.OrderIndex == orderIndex);

        if (duplicated != null)
            throw new ValidationException("OrderIndex phải là duy nhất trong mô-đun.");
    }
}
