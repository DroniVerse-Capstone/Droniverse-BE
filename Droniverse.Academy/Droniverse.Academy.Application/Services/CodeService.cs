using AutoMapper;
using Droniverse.Academy.Application.DomainEvent;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Domain.QueryModels;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services.IServices;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Droniverse.Academy.Application.Services;

public class CodeService : ICodeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CodeService> _logger;
    private readonly CommunityMicroserviceClient _communityMicroserviceClient;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly IEmailService _emailService;
    private readonly IClock _clock;
    private readonly IMediator _mediator;

    public CodeService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CodeService> logger,
        CommunityMicroserviceClient communityMicroserviceClient,
        IdentityMicroserviceClient identityMicroserviceClient,
        IEmailService emailService,
        IClock clock,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _communityMicroserviceClient = communityMicroserviceClient;
        _identityMicroserviceClient = identityMicroserviceClient;
        _emailService = emailService;
        _clock = clock;
        _mediator = mediator;
    }

    public async Task<CreateCodesResponse> CreateCodeAsync(GenerateCodesRequestDTO request)
    {
        if (request is null)
            throw new ValidationException("Dữ liệu tạo mã code không hợp lệ.");

        var currentUserId = _currentUserService.UserId;

        CourseInfoQueryModel? courseInfo = await _unitOfWork.Courses.GetCourseInfoByIdAsync(request.CourseId);

        if (courseInfo is null)
            throw new NotFoundException($"Không tìm thấy khóa học");

        var now = _clock.Now;
        var courseNameForCode = !string.IsNullOrWhiteSpace(courseInfo.CourseNameEN)
            ? courseInfo.CourseNameEN
            : courseInfo.CourseNameVN;

        var generatedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var codes = new List<Code>(request.Quantity);

        while (codes.Count < request.Quantity)
        {
            var codeId = GenerateCodeId(courseNameForCode);
            if (!generatedIds.Add(codeId))
            {
                continue;
            }

            var code = new Code(
                codeId: codeId,
                clubId: request.ClubId,
                courseId: request.CourseId,
                expireDate: now.AddMonths(6),
                createdBy: currentUserId,
                now: now);

            codes.Add(code);
        }

        await _unitOfWork.Codes.AddRangeAsync(codes);
        await _unitOfWork.SaveChangesAsync();

        return new CreateCodesResponse
        {
            CreatedCode = codes.Count
        };

    }

    private string GenerateCodeId(string courseName)
    {
        // Lấy 4 ký tự đầu tiên trong courseName
        string prefix = courseName.Substring(0, Math.Min(4, courseName.Length)).ToUpper();

        // Nếu courseName < 4 ký tự, pad thêm ký tự
        while (prefix.Length < 4)
        {
            prefix += GenerateRandomChar();
        }

        //Lấy ddMMyyyy
        string datePart = DateTime.UtcNow.AddHours(7).ToString("ddMMyy");

        //Generate 2 phần ngẫu nhiên
        string randomPart1 = GenerateRandomPart();
        string randomPart2 = GenerateRandomPart();

        return $"{prefix}-{datePart}-{randomPart1}-{randomPart2}";

    }

    private string GenerateRandomPart()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 4)
        .Select(_ => chars[random.Next(chars.Length)])
        .ToArray());
    }

    private char GenerateRandomChar()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Random random = new Random();
        return chars[random.Next(chars.Length)];
    }

    public Task<CodeResponseDTO> DeleteCodeAsync(string codeId)
    {
        throw new NotImplementedException();
    }

    public async Task<PaginationResult<IEnumerable<CodeResponseDTO>>> GetAllCodesAsync(CodeSearchRequestDTO requestDTO)
    {
        requestDTO ??= new CodeSearchRequestDTO();

        var pageIndex = requestDTO.CurrentPage < 1 ? 1 : requestDTO.CurrentPage;
        var pageSize = requestDTO.PageSize < 1 ? 5 : requestDTO.PageSize;

        PaginationResult<IEnumerable<Code>> codes = await _unitOfWork.Codes.GetAllCodesAsync(
            requestDTO,
            pageIndex,
            pageSize);

        var mappedCodes = codes.Data.Select(MapCodeResponse).ToList();

        return new PaginationResult<IEnumerable<CodeResponseDTO>>(
            mappedCodes,
            codes.TotalRecords,
            codes.PageIndex,
            codes.PageSize
        );
    }

    public async Task<CodeResponseDTO> GetCodeAsync(string codeId)
    {
        Code? code = await _unitOfWork.Codes.GetByIdAsync(codeId);
        if (code is null)
        {
            throw new NotFoundException($"Code with id {codeId} not found");
        }

        return MapCodeResponse(code);
    }

    public Task<CodeResponseDTO> UpdateCodeAsync(string codeId)
    {
        throw new NotImplementedException();
    }

    public async Task<ClubCodesResponse> GetCodesByClub(Guid clubId, GetAllCodesByClubSearchRequest request)
    {
        request ??= new GetAllCodesByClubSearchRequest();

        var pageIndex = request.CurrentPage;
        var pageSize = request.PageSize;

        var pagedCodes = await _unitOfWork.Codes.GetCodesByClubAsync(clubId, request, pageIndex, pageSize);
        var codes = pagedCodes.Data.ToList();

        var usedUserIds = codes
                 .Where(c => c.IsUsed())
                 .Select(c => c.UsedByUserID!.Value)
                 .Distinct()
                 .ToList();

        var users = usedUserIds.Count > 0
            ? await _identityMicroserviceClient.GetUsersBulk(usedUserIds)
            : [];

        var usersById = users.ToDictionary(
            x => x.UserId,
            x => new SimpleUserReponse
            {
                UserId = x.UserId,
                FullName = AppHelper.GetFullName(x) ?? x.Username,
                Email = x.Email,
                AvatarUrl = x.ImageUrl
            });

        var codeItems = codes.Select(code =>
        {
            var isUsed = code.IsUsed();
            var consumer = isUsed && code.UsedByUserID.HasValue
                ? usersById.GetValueOrDefault(code.UsedByUserID.Value)
                : null;

            return new CodeEntryResponse
            {
                Code = code.CodeID,
                CourseID = code.CourseID.ToString(),
                IsUsed = isUsed,
                ComsumerInfo = consumer,
                ExpireDate = code.ExpireDate
            };
        }).ToList();

        return new ClubCodesResponse
        {
            ClubID = clubId.ToString(),
            TotalItems = pagedCodes.TotalRecords,
            CodesItem = codeItems
        };
    }

    /// <summary>
    /// Club member nhập mã code của khóa học
    /// </summary>
    /// <param name="codeId"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="ValidationException"></exception>
    public async Task<CodeUsageResponseDTO> EnterCodeAsync(string codeId)
    {
        try
        {
            ClaimsPrincipal user = _currentUserService.User;
            if (user is null)
            {
                throw new UnauthorizedAccessException("Người dùng chưa xác thực");
            }

            Code? code = await _unitOfWork.Codes.GetByConditionAsync(c => c.CodeID == codeId);
            if (code is null)
            {
                throw new ValidationException("Mã code của khóa học chưa đúng! Vui lòng nhập lại");
            }

            if (code.OwnedUserID.HasValue && code.OwnedUserID.Value != _currentUserService.UserId)
            {
                throw new ValidationException("Mã code này đã được gán cho người dùng khác.");
            }

            code.Redeem(_currentUserService.UserId, _clock.Now);

            await _unitOfWork.Codes.UpdateAsync(code);
            await _unitOfWork.SaveChangesAsync();

            //Gọi qua community để gọi hàm consumeslot
            await _communityMicroserviceClient.ConsumeSlotForCodeAsync(code.ClubID, code.CourseID, 1);

            return new CodeUsageResponseDTO
            {
                CodeID = code.CodeID,
                UserID = _currentUserService.UserId,
                UsedDate = code.UsedDate ?? _clock.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while entering code {CodeID} for user {UserID}", codeId, _currentUserService.UserId);
            throw;
        }
    }

    public async Task<CodeAssignmentResponseDTO> AssignCodeAsync(AssignCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CodeId))
            throw new ValidationException("CodeId không hợp lệ.");

        if (request.UserId == Guid.Empty)
            throw new ValidationException("UserId không hợp lệ.");

        var code = await _unitOfWork.Codes.GetByIdAsync(request.CodeId.Trim())
            ?? throw new NotFoundException("Không tìm thấy mã code.");

        code.AssignToUser(request.UserId, _clock.Now);

        await _unitOfWork.Codes.UpdateAsync(code);
        await _unitOfWork.SaveChangesAsync();

        if (request.SendEmail)
        {
            var userTask = _identityMicroserviceClient.GetUsersBulk([request.UserId]);
            var courseTask = _unitOfWork.Courses.GetCourseInfoByIdAsync([code.CourseID]);

            await Task.WhenAll(userTask, courseTask);

            var user = (await userTask).FirstOrDefault();
            var course = (await courseTask)?.FirstOrDefault();

            if (user != null && course != null)
            {
                var @event = new CodeAssignedEvent(
                    code: code.CodeID,
                    userId: request.UserId,
                    courseId: code.CourseID,
                    email: user.Email,
                    fullName: AppHelper.GetFullName(user)!,
                    courseNameVN: course.CourseNameVN,
                    courseNameEN: course.CourseNameEN
                );

                await _mediator.Publish(@event);
            }
            else
            {
                _logger.LogWarning(
                    "Không gửi được email. User hoặc Course không tồn tại. UserId: {UserId}, CourseId: {CourseId}",
                    request.UserId,
                    code.CourseID
                );
            }
        }

        return new CodeAssignmentResponseDTO
        {
            CodeId = code.CodeID,
            UserId = request.UserId,
            AssignedAt = code.UpdatedAt ?? _clock.Now
        };
    }

    public async Task<BulkCodeAssignmentResponseDTO> BulkAssignCodesAsync(BulkAssignCodesRequest request)
    {
        if (request?.Items == null || request.Items.Count == 0)
            throw new ValidationException("Danh sách gán code không được để trống.");

        var items = request.Items
            .Where(x => !string.IsNullOrWhiteSpace(x.CodeId) && x.UserId != Guid.Empty)
            .Select(x => new BulkAssignCodeItemRequest
            {
                CodeId = x.CodeId.Trim(),
                UserId = x.UserId
            })
            .ToList();

        if (items.Count == 0)
            throw new ValidationException("Danh sách gán code không hợp lệ.");

        var duplicateCodeIds = items
            .GroupBy(x => x.CodeId, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateCodeIds.Count > 0)
            throw new ValidationException($"Danh sách có mã code bị trùng: {string.Join(", ", duplicateCodeIds)}");

        var codes = (await _unitOfWork.Codes.GetByCodeIdsAsync(items.Select(x => x.CodeId))).ToList();
        var codeById = codes.ToDictionary(x => x.CodeID, StringComparer.OrdinalIgnoreCase);

        var missingCodeIds = items
            .Where(x => !codeById.ContainsKey(x.CodeId))
            .Select(x => x.CodeId)
            .ToList();

        if (missingCodeIds.Count > 0)
            throw new NotFoundException($"Không tìm thấy mã code: {string.Join(", ", missingCodeIds)}");

        var assignedItems = new List<CodeAssignmentResponseDTO>(items.Count);

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            foreach (var item in items)
            {
                var code = codeById[item.CodeId];
                code.AssignToUser(item.UserId, _clock.Now);

                assignedItems.Add(new CodeAssignmentResponseDTO
                {
                    CodeId = code.CodeID,
                    UserId = item.UserId,
                    AssignedAt = code.UpdatedAt ?? _clock.Now
                });
            }

            await _unitOfWork.SaveChangesAsync();
        });

        if (request.SendEmail)
        {
            var userIds = assignedItems
                .Select(x => x.UserId)
                .Distinct()
                .ToList();

            var courseIds = assignedItems
                .Select(x => codeById[x.CodeId].CourseID)
                .Distinct()
                .ToList();

            // chạy song song cho nhanh
            var userTask = _identityMicroserviceClient.GetUsersBulk(userIds);
            var courseTask = _unitOfWork.Courses.GetCourseInfoByIdAsync(courseIds);

            await Task.WhenAll(userTask, courseTask);

            var users = (await userTask).ToDictionary(x => x.UserId);

            var coursesDict = (await courseTask ?? Enumerable.Empty<CourseInfoQueryModel>())
                .ToDictionary(x => x.CourseId);

            var tasks = assignedItems.Select(async x =>
            {
                var code = codeById[x.CodeId];

                if (!users.TryGetValue(x.UserId, out var user))
                    return;

                if (!coursesDict.TryGetValue(code.CourseID, out var course))
                    return;

                var @event = new CodeAssignedEvent(
                    code: code.CodeID,
                    userId: x.UserId,
                    courseId: code.CourseID,
                    email: user.Email,
                    fullName: AppHelper.GetFullName(user)!,
                    courseNameVN: course.CourseNameVN,
                    courseNameEN: course.CourseNameEN
                );

                await _mediator.Publish(@event);
            });

            await Task.WhenAll(tasks);
        }

        return new BulkCodeAssignmentResponseDTO
        {
            TotalAssigned = assignedItems.Count,
            AssignedItems = assignedItems
        };
    }

    public async Task<PaginationResult<IEnumerable<MyCodeResponseDTO>>> GetCodesByUserAsync(Guid userId, GetCodesByUserSearchRequest request)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("UserId không hợp lệ.");

        request ??= new GetCodesByUserSearchRequest();

        var pageIndex = request.CurrentPage < 1 ? 1 : request.CurrentPage;
        var pageSize = request.PageSize < 1 ? 5 : request.PageSize;

        var pagedCodes = await _unitOfWork.Codes.GetCodesByUserAsync(userId, pageIndex, pageSize, request.IsUsed);

        var items = pagedCodes.Data
            .Select(c => new MyCodeResponseDTO
            {
                CodeId = c.CodeID,
                ClubId = c.ClubID,
                CourseId = c.CourseID,
                Status = c.Status,
                ExpireDate = c.ExpireDate,
                UsedDate = c.UsedDate
            })
            .ToList();

        return new PaginationResult<IEnumerable<MyCodeResponseDTO>>(
            items,
            pagedCodes.TotalRecords,
            pagedCodes.PageIndex,
            pagedCodes.PageSize);
    }

    private async Task SendAssignEmailAsync(Guid userId, string codeId, Guid courseId)
    {
        var user = (await _identityMicroserviceClient.GetUsersBulk([userId])).FirstOrDefault();
        if (user == null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        var fullName = AppHelper.GetFullName(user) ?? user.Username;
        var subject = "Bạn vừa được cấp mã học khóa học";
        var message = $"Xin chào {fullName},<br/>Bạn vừa được cấp mã <b>{codeId}</b> cho khóa học <b>{courseId}</b>.";
        await _emailService.SendEmailAsync(user.Email, subject, message);
    }

    private static CodeResponseDTO MapCodeResponse(Code code)
    {
        return new CodeResponseDTO
        {
            CodeID = code.CodeID,
            CourseID = code.CourseID.ToString(),
            ClubID = code.ClubID.ToString(),
            OwnedUserID = code.OwnedUserID,
            UsedByUserID = code.UsedByUserID,
            UsedDate = code.UsedDate,
            ExpireDate = code.ExpireDate,
            Status = code.Status
        };
    }
}

