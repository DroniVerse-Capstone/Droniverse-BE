using AutoMapper;
using Droniverse.Academy.Application.DomainEvent;
using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Domain.QueryModels;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
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
    private readonly IMapper _mapper;
    private readonly CommunityMicroserviceClient _communityMicroserviceClient;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly IEmailService _emailService;
    private readonly IClock _clock;
    private readonly IMediator _mediator;

    public CodeService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CodeService> logger,
        IMapper mapper,
        CommunityMicroserviceClient communityMicroserviceClient,
        IdentityMicroserviceClient identityMicroserviceClient,
        IEmailService emailService,
        IClock clock,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _mapper = mapper;
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

        var courseInfo = await _unitOfWork.Courses.GetCourseInfoByIdAsync(request.CourseId)
            ?? throw new NotFoundException("Không tìm thấy khóa học");

        var now = _clock.Now;

        //  1. Consume trước
        var clubCourseUpdated = await _communityMicroserviceClient
            .ConsumeSlotCrossAsync(request.ClubId, request.CourseId, request.Quantity)
            ?? throw new NotFoundException("Không tìm thấy club-course");

        //  2. Generate code
        var courseNameForCode = !string.IsNullOrWhiteSpace(courseInfo.CourseNameEN)
            ? courseInfo.CourseNameEN
            : courseInfo.CourseNameVN;

        var generatedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var codes = new List<Code>(request.Quantity);

        while (codes.Count < request.Quantity)
        {
            var codeId = GenerateCodeId(courseNameForCode);
            if (!generatedIds.Add(codeId)) continue;

            codes.Add(new Code(
                codeId,
                request.ClubId,
                request.CourseId,
                now.AddMonths(6),
                currentUserId,
                now));
        }

        await _unitOfWork.Codes.AddRangeAsync(codes);
        await _unitOfWork.SaveChangesAsync();

        return new CreateCodesResponse
        {
            CreatedCode = codes.Count,
            ClubCourse = clubCourseUpdated
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

    public async Task<AllCodesWithOverviewDto> GetAllCodesAsync(CodeSearchRequestDTO requestDTO)
    {
        requestDTO ??= new CodeSearchRequestDTO();

        var pageIndex = requestDTO.CurrentPage < 1 ? 1 : requestDTO.CurrentPage;
        var pageSize = requestDTO.PageSize < 1 ? 5 : requestDTO.PageSize;

        PaginationResult<IEnumerable<Code>> codes = await _unitOfWork.Codes.GetAllCodesAsync(
            requestDTO,
            pageIndex,
            pageSize);

        var mappedCodes = await MapCodeResponsesAsync(codes.Data);

        var overviewCounts = await _unitOfWork.Codes.GetCodeOverviewAsync();
        var overview = new CodeOverviewDto(
            overviewCounts.TotalCodes,
            overviewCounts.AvailableCodes,
            overviewCounts.UsedCodes,
            overviewCounts.ExpiredCodes);

        var paginationResult = new PaginationResult<IEnumerable<CodeResponseDTO>>(
            mappedCodes,
            codes.TotalRecords,
            codes.PageIndex,
            codes.PageSize
        );

        return new AllCodesWithOverviewDto(overview, paginationResult);
    }

    public async Task<CodeResponseDTO> GetCodeAsync(string codeId)
    {
        Code? code = await _unitOfWork.Codes.GetByIdAsync(codeId);
        if (code is null)
        {
            throw new NotFoundException($"Code with id {codeId} not found");
        }

        return (await MapCodeResponsesAsync([code])).First();
    }

    public Task<CodeResponseDTO> UpdateCodeAsync(string codeId)
    {
        throw new NotImplementedException();
    }

    public async Task<ClubCodesResponse> GetCodesByClub(Guid clubId, Guid courseId, GetAllCodesByClubSearchRequest request)
    {
        if (clubId == Guid.Empty)
            throw new ValidationException("ClubId không hợp lệ.");

        if (courseId == Guid.Empty)
            throw new ValidationException("CourseId không hợp lệ.");

        request ??= new GetAllCodesByClubSearchRequest();

        var pageIndex = request.CurrentPage;
        var pageSize = request.PageSize;

        var pagedCodes = await _unitOfWork.Codes.GetCodesByClubAsync(clubId, courseId, request, pageIndex, pageSize);
        var codes = pagedCodes.Data.ToList();

        var courseInfo = (await _unitOfWork.Courses.GetSimpleCoursesByIdsAsync([courseId])).FirstOrDefault()
            ?? throw new NotFoundException("Không tìm thấy thông tin khóa học.");

        var userIds = codes
                 .Select(c => c.UsedByUserID)
                 .Where(x => x.HasValue && x.Value != Guid.Empty)
                 .Select(x => x!.Value)
                 .Distinct()
                 .ToList();

        var users = userIds.Count > 0
            ? await _identityMicroserviceClient.GetUsersBulk(userIds)
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
            var consumer = code.IsUsed() && code.UsedByUserID.HasValue
                ? usersById.GetValueOrDefault(code.UsedByUserID.Value)
                : null;

            return new CodeEntryResponse
            {
                Code = code.CodeID,
                OwnerInfo = null,
                ComsumerInfo = consumer,
                ExpireDate = code.ExpireDate
            };
        }).ToList();

        return new ClubCodesResponse
        {
            ClubID = clubId.ToString(),
            CourseInfo = courseInfo,
            CodesItem = new PaginationResult<IEnumerable<CodeEntryResponse>>(
                codeItems,
                pagedCodes.TotalRecords,
                pagedCodes.PageIndex,
                pagedCodes.PageSize)
        };
    }

    /// <summary>
    /// Club member nhập mã code của khóa học
    /// </summary>
    /// <param name="codeId"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="ValidationException"></exception>
    public async Task<CodeUsageResponseDTO> EnterCodeAsync(Guid clubId, string codeId)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;

            var isParticipant = await _communityMicroserviceClient.CheckParticipantByClubAsync(clubId, currentUserId, ParticipationStatus.ACTIVE);

            if (!isParticipant)
                throw new ValidationException("Người dùng không phải thành viên của câu lạc bộ");

            Code? code = await _unitOfWork.Codes.GetByConditionAsync(c => c.CodeID == codeId);
            if (code is null)
                throw new ValidationException("Mã code của khóa học chưa đúng! Vui lòng nhập lại");

            if (code.Status == CodeStatus.USED)
                throw new ValidationException("Mã code đã được sử dụng");

            code.Redeem(_currentUserService.UserId, _clock.Now);

            await _unitOfWork.Codes.UpdateAsync(code);
            await _unitOfWork.SaveChangesAsync();

            //Gọi qua community để gọi hàm consumeslot
            //await _communityMicroserviceClient.ConsumeSlotForCodeAsync(code.ClubID, code.CourseID, 1);

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

        code.Redeem(request.UserId, _clock.Now);

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
            AssignedAt = code.UsedDate ?? _clock.Now
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
                code.Redeem(item.UserId, _clock.Now);

                assignedItems.Add(new CodeAssignmentResponseDTO
                {
                    CodeId = code.CodeID,
                    UserId = item.UserId,
                    AssignedAt = code.UsedDate ?? _clock.Now
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

    public async Task<PaginationResult<IEnumerable<MyCodeResponseDTO>>> GetCodesByUserAsync(GetCodesByUserSearchRequest request)
    {
        var currentUserId = _currentUserService.UserId;

        request ??= new GetCodesByUserSearchRequest();

        var pageIndex = request.CurrentPage;
        var pageSize = request.PageSize;

        var pagedCodes = await _unitOfWork.Codes.GetCodesByUserAsync(currentUserId, pageIndex, pageSize, request.IsUsed);

        var codes = pagedCodes.Data.ToList();

        var clubIds = codes
            .Select(c => c.ClubID)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var courseIds = codes
            .Select(c => c.CourseID)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var clubsTask = _communityMicroserviceClient.GetClubInfoBulkAsync(clubIds);
        var coursesTask = _unitOfWork.Courses.GetSimpleCoursesByIdsAsync(courseIds);

        await Task.WhenAll(clubsTask, coursesTask);

        var clubsById = (await clubsTask)
            .Where(x => x != null && x.ClubId != Guid.Empty)
            .GroupBy(x => x.ClubId)
            .ToDictionary(g => g.Key, g => g.First());

        var coursesById = (await coursesTask)
            .Where(x => x != null && x.CourseId != Guid.Empty)
            .GroupBy(x => x.CourseId)
            .ToDictionary(g => g.Key, g => g.First());

        var items = codes
            .Select(c => new MyCodeResponseDTO
            {
                CodeId = c.CodeID,
                ClubInfo = clubsById.TryGetValue(c.ClubID, out var club)
                    ? club
                    : new SimpleClubResponse
                    {
                        ClubId = c.ClubID,
                        ClubNameVN = string.Empty,
                        ClubNameEN = string.Empty,
                        ImageUrl = string.Empty
                    },
                CourseId = coursesById.TryGetValue(c.CourseID, out var course)
                    ? course
                    : new SimpleCourseResponse
                    {
                        CourseId = c.CourseID,
                        CourseNameVN = string.Empty,
                        CourseNameEN = string.Empty,
                        ImageUrl = string.Empty
                    },
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

    public async Task<PaginationResult<IEnumerable<SimpleUserReponse>>> GetUsersCode(
      Guid clubId,
      Guid courseId,
      GetUsersNoCodesSearchRequest request)
    {
        if (clubId == Guid.Empty)
            throw new ValidationException("ClubId không hợp lệ.");

        if (courseId == Guid.Empty)
            throw new ValidationException("CourseId không hợp lệ.");

        request ??= new GetUsersNoCodesSearchRequest();

        var pageIndex = request.CurrentPage;
        var pageSize = request.PageSize;

        var course = await _unitOfWork.Courses.GetByIdAsync(courseId)
            ?? throw new NotFoundException("Không tìm thấy khóa học.");

        var courseOwnership = await _communityMicroserviceClient
            .GetRemainingQuantityRawAsync(clubId, courseId);

        if (courseOwnership == null)
            throw new ValidationException("Câu lạc bộ chưa sở hữu khóa học này.");

        var ownedUserIds = await _unitOfWork.Codes
            .GetOwnedUserIdsByClubAndCourseAsync(clubId, courseId);

        IEnumerable<Guid> candidateUserIds;
        var userCodesFilter = request.UserCodes ?? UserCodesEnum.User_Has_Codes;

        if (userCodesFilter == UserCodesEnum.User_Has_Codes)
        {
            candidateUserIds = ownedUserIds;
        }
        else
        {
            var participantIds = await _communityMicroserviceClient
                .GetClubParticipantIdsAsync(clubId, ParticipationStatus.ACTIVE);

            var ownedSet = ownedUserIds.ToHashSet();

            candidateUserIds = participantIds
                .Where(x => x != Guid.Empty && !ownedSet.Contains(x));
        }

        var candidateSet = candidateUserIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToHashSet();

        // 4. SEARCH (dùng API pagination từ Identity)
        if (!string.IsNullOrWhiteSpace(request.FullName) ||
    !string.IsNullOrWhiteSpace(request.Email))
        {
            var searchResult = await _identityMicroserviceClient
                .SearchUsersWithPaginationAsync(new SearchUsersWithPaginationRequest
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    UserIds = candidateSet.ToList(),
                    CurrentPage = pageIndex,
                    PageSize = pageSize
                });

            return new PaginationResult<IEnumerable<SimpleUserReponse>>(
                searchResult.Items,
                searchResult.TotalItems,
                pageIndex,
                pageSize);
        }

        // 5. KHÔNG SEARCH → xử lý local pagination
        var orderedIds = candidateSet.ToList();

        var totalRecords = orderedIds.Count;

        var pagedUserIds = orderedIds
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        if (pagedUserIds.Count == 0)
        {
            return new PaginationResult<IEnumerable<SimpleUserReponse>>(
                [],
                totalRecords,
                pageIndex,
                pageSize);
        }

        var users = await _identityMicroserviceClient.GetUsersBulk(pagedUserIds);

        var usersById = users.ToDictionary(x => x.UserId, x => x);

        var items = pagedUserIds
            .Where(usersById.ContainsKey)
            .Select(id => usersById[id])
            .Select(u => new SimpleUserReponse
            {
                UserId = u.UserId,
                FullName = AppHelper.GetFullName(u) ?? u.Username,
                Email = u.Email,
                AvatarUrl = u.ImageUrl
            })
            .ToList();

        return new PaginationResult<IEnumerable<SimpleUserReponse>>(
            items,
            totalRecords,
            pageIndex,
            pageSize);
    }

    public async Task<CodeResponseDTO> CreateWithAssignCodeAsync(Shared.DTOs.Request.GenerateWithAssignCodeRequestDTO request)
    {
        if (request is null)
            throw new ValidationException("Dữ liệu tạo mã code không hợp lệ.");

        if (request.ClubId == Guid.Empty)
            throw new ValidationException("ClubId không hợp lệ.");

        if (request.CourseId == Guid.Empty)
            throw new ValidationException("CourseId không hợp lệ.");

        if (request.Quantity <= 0)
            throw new ValidationException("Số lượng mã code phải lớn hơn 0.");

        if (request.Quantity != 1)
            throw new ValidationException("API này chỉ hỗ trợ tạo và gán 1 mã code cho người dùng hiện tại.");

        var currentUserId = _currentUserService.UserId;

        var courseInfo = await _unitOfWork.Courses.GetCourseInfoByIdAsync(request.CourseId)
            ?? throw new NotFoundException("Không tìm thấy khóa học");

        var now = _clock.Now;

        var courseNameForCode = !string.IsNullOrWhiteSpace(courseInfo.CourseNameEN)
            ? courseInfo.CourseNameEN
            : courseInfo.CourseNameVN;

        var code = new Code(
            GenerateCodeId(courseNameForCode),
            request.ClubId,
            request.CourseId,
            now.AddMonths(6),
            currentUserId,
            now);

        //code.Redeem(currentUserId, now);

        await _unitOfWork.Codes.AddAsync(code);
        await _unitOfWork.SaveChangesAsync();

        if (request.Email != null && !string.IsNullOrWhiteSpace(request.Email))
        {
            var @event = new CodeAssignedEvent(
                code: code.CodeID,
                userId: currentUserId,
                courseId: code.CourseID,
                email: request.Email,
                fullName: request.FullName,
                courseNameVN: courseInfo.CourseNameVN,
                courseNameEN: courseInfo.CourseNameEN);

            await _mediator.Publish(@event);
        }
        else
        {
            _logger.LogWarning(
                "Không gửi được email khi tạo và gán code cho user hiện tại. UserId: {UserId}",
                currentUserId);
        }

        _logger.LogInformation(
            "Đã tạo và gán mã code {CodeId} cho người dùng hiện tại {UserId}: ",
            code.CodeID,
            currentUserId);

        return (await MapCodeResponsesAsync([code])).First();
    }

    public async Task<GetCodeByUsersResponseDTO> GetCodeByUsers(Guid clubId, Guid courseId)
    {
        if (clubId == Guid.Empty)
            throw new ValidationException("ClubId không hợp lệ.");

        if (courseId == Guid.Empty)
            throw new ValidationException("CourseId không hợp lệ.");

        var currentUserId = _currentUserService.UserId;
        if (currentUserId == Guid.Empty)
            throw new UnauthorizedAccessException("Người dùng chưa xác thực.");

        var now = _clock.Now;

        var checkParticipantTask = _communityMicroserviceClient
            .CheckParticipantByClubAsync(clubId, currentUserId);

        var clubCourseTask = _communityMicroserviceClient
            .GetRemainingQuantityRawAsync(clubId, courseId);

        await Task.WhenAll(checkParticipantTask, clubCourseTask);

        var isParticipant = await checkParticipantTask;
        if (!isParticipant)
            throw new NotFoundException("Người dùng chưa tham gia câu lạc bộ này");

        var clubCourse = await clubCourseTask;
        if (clubCourse is null)
            throw new NotFoundException("Không tìm thấy thông tin sở hữu khóa học của câu lạc bộ.");

        if (clubCourse.ProfitType == ClubCourseProfit.PROFIT)
            throw new ValidationException("Khóa học này đang tính phí, không thể nhận mã miễn phí.");

        if (clubCourse.RemainingQuantity <= 0)
            throw new InvalidOperationException("Không còn đủ mã để nhận.");

        var hasOwnedCode = await _unitOfWork.Codes
            .HasActiveUnusedOwnedCodeAsync(clubId, courseId, currentUserId, now);

        if (hasOwnedCode)
            throw new ValidationException("Bạn đã có mã code chưa sử dụng cho khóa học này.");

        var courseInfo = await _unitOfWork.Courses
            .GetCourseInfoByIdAsync(courseId)
            ?? throw new NotFoundException("Không tìm thấy khóa học.");

        var consumed = await _communityMicroserviceClient
            .ConsumeSlotCrossAsync(clubId, courseId, quantity: 1)
            ?? throw new NotFoundException("Không tìm thấy khóa học và câu lạc bộ.");

        var courseNameForCode = !string.IsNullOrWhiteSpace(courseInfo.CourseNameEN)
            ? courseInfo.CourseNameEN
            : courseInfo.CourseNameVN;

        var code = new Code(
            GenerateCodeId(courseNameForCode),
            clubId,
            courseId,
            now.AddMonths(6),
            currentUserId,
            now
        );

        code.Redeem(currentUserId, now);

        await _unitOfWork.Codes.AddAsync(code);
        await _unitOfWork.SaveChangesAsync();

        var currentUser = (await _identityMicroserviceClient
            .GetUsersBulk([currentUserId]))
            .FirstOrDefault();

        if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.Email))
        {
            var @event = new CodeAssignedEvent(
                code: code.CodeID,
                userId: currentUserId,
                courseId: courseId,
                email: currentUser.Email,
                fullName: AppHelper.GetFullName(currentUser) ?? currentUser.Username,
                courseNameVN: courseInfo.CourseNameVN,
                courseNameEN: courseInfo.CourseNameEN
            );

            await _mediator.Publish(@event);
        }
        else
        {
            _logger.LogWarning("Không gửi được email nhận code cho user {UserId}", currentUserId);
        }

        return new GetCodeByUsersResponseDTO
        {
            RemainingCode = consumed.RemainingQuantity
        };
    }

    //private async Task SendAssignEmailAsync(Guid userId, string scodeId, Guid courseId)
    //{
    //    var user = (await _identityMicroserviceClient.GetUsersBulk([userId])).FirstOrDefault();
    //    if (user == null || string.IsNullOrWhiteSpace(user.Email))
    //    {
    //        return;
    //    }

    //    var fullName = AppHelper.GetFullName(user) ?? user.Username;
    //    var subject = "Bạn vừa được cấp mã học khóa học";
    //    var message = $"Xin chào {fullName},<br/>Bạn vừa được cấp mã <b>{codeId}</b> cho khóa học <b>{courseId}</b>.";
    //    await _emailService.SendEmailAsync(user.Email, subject, message);
    //}

    private async Task<List<CodeResponseDTO>> MapCodeResponsesAsync(IEnumerable<Code> codes)
    {
        var codeList = codes?.ToList() ?? [];

        if (codeList.Count == 0)
        {
            return [];
        }

        var clubIds = codeList
            .Select(code => code.ClubID)
            .Where(clubId => clubId != Guid.Empty)
            .Distinct()
            .ToList();

        var ownerUserIds = codeList
            .Select(code => code.CreatedBy)
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToList();

        var usedByUserIds = codeList
            .Select(code => code.UsedByUserID)
            .Where(userId => userId.HasValue && userId.Value != Guid.Empty)
            .Select(userId => userId!.Value)
            .Distinct()
            .ToList();

        var courseIds = codeList
            .Select(code => code.CourseID)
            .Where(courseId => courseId != Guid.Empty)
            .Distinct()
            .ToList();

        Task<IEnumerable<SimpleClubResponse>> clubsTask = clubIds.Count > 0
            ? _communityMicroserviceClient.GetClubInfoBulkAsync(clubIds)
            : Task.FromResult<IEnumerable<SimpleClubResponse>>(Array.Empty<SimpleClubResponse>());

        Task<IEnumerable<UserResponse>> ownerUsersTask = ownerUserIds.Count > 0
            ? _identityMicroserviceClient.GetUsersBulk(ownerUserIds)
            : Task.FromResult<IEnumerable<UserResponse>>(Array.Empty<UserResponse>());

        Task<IEnumerable<UserResponse>> usedByUsersTask = usedByUserIds.Count > 0
            ? _identityMicroserviceClient.GetUsersBulk(usedByUserIds)
            : Task.FromResult<IEnumerable<UserResponse>>(Array.Empty<UserResponse>());

        Task<PaginationResult<IEnumerable<Course>>> coursesTask = courseIds.Count > 0
            ? _unitOfWork.Courses.GetAllWithAllVersionsAsync(
                c => courseIds.Contains(c.CourseID),
                pageIndex: 1,
                pageSize: courseIds.Count)
            : Task.FromResult(new PaginationResult<IEnumerable<Course>>([], 0, 1, 0));

        await Task.WhenAll(clubsTask, ownerUsersTask, usedByUsersTask, coursesTask);

        var clubsById = (await clubsTask)
            .Where(club => club != null && club.ClubId != Guid.Empty)
            .GroupBy(club => club.ClubId)
            .ToDictionary(group => group.Key, group => group.First());

        var ownerUsersById = (await ownerUsersTask)
            .Where(user => user != null && user.UserId != Guid.Empty)
            .GroupBy(user => user.UserId)
            .ToDictionary(group => group.Key, group => group.First());

        var usedByUsersById = (await usedByUsersTask)
            .Where(user => user != null && user.UserId != Guid.Empty)
            .GroupBy(user => user.UserId)
            .ToDictionary(group => group.Key, group => group.First());

        var coursesById = (await coursesTask).Data
            .ToList()
            .GroupBy(course => course.CourseID)
            .ToDictionary(group => group.Key, group => group.First());

        var courseCreatorIds = coursesById.Values
            .Select(course => course.CreateBy)
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToList();

        var courseUpdaterIds = coursesById.Values
            .SelectMany(course =>
                new[] { course.CurrentVersion?.UpdateBy }
                    .Concat(course.CourseVersions?.Select(version => version.UpdateBy) ?? []))
            .Where(userId => userId.HasValue && userId.Value != Guid.Empty)
            .Select(userId => userId!.Value)
            .Distinct()
            .ToList();

        var courseUserIds = courseCreatorIds
            .Concat(courseUpdaterIds)
            .Distinct()
            .ToList();

        var courseUsers = courseUserIds.Count > 0
            ? await _identityMicroserviceClient.GetUsersBulk(courseUserIds)
            : [];

        var courseUsersById = courseUsers
            .ToDictionary(user => user.UserId, user => user);

        return codeList.Select(code => new CodeResponseDTO
        {
            CodeID = code.CodeID,
            Course = coursesById.TryGetValue(code.CourseID, out var course)
                ? MapCourseDetail(course, courseUsersById)
                : null,
            Club = clubsById.TryGetValue(code.ClubID, out var club)
                ? club
                : null,
            OwnerUser = ownerUsersById.TryGetValue(code.CreatedBy, out var ownerUser)
                ? ownerUser
                : null,
            UsedByUser = code.UsedByUserID.HasValue && usedByUsersById.TryGetValue(code.UsedByUserID.Value, out var usedByUser)
                ? usedByUser
                : null,
            UsedDate = code.UsedDate,
            ExpireDate = code.ExpireDate,
            Status = code.Status
        }).ToList();
    }

    private CourseDetailResponseDTO MapCourseDetail(
        Course course,
        IReadOnlyDictionary<Guid, UserResponse> usersById)
    {
        var courseDetail = _mapper.Map<CourseDetailResponseDTO>(course);

        if (usersById.TryGetValue(course.CreateBy, out var creator))
        {
            courseDetail.Creator = new SimpleUserReponse
            {
                UserId = creator.UserId,
                FullName = AppHelper.GetFullName(creator) ?? creator.Username,
                Email = creator.Email,
                AvatarUrl = creator.ImageUrl
            };
        }

        if (courseDetail.CurrentVersion != null && course.CurrentVersion?.UpdateBy is Guid currentUpdaterId &&
            usersById.TryGetValue(currentUpdaterId, out var currentUpdater))
        {
            courseDetail.CurrentVersion.Updater = new SimpleUserReponse
            {
                UserId = currentUpdater.UserId,
                FullName = AppHelper.GetFullName(currentUpdater) ?? currentUpdater.Username,
                Email = currentUpdater.Email,
                AvatarUrl = currentUpdater.ImageUrl
            };
        }

        if (courseDetail.CourseVersions.Count > 0 && course.CourseVersions != null)
        {
            var versionEntitiesById = course.CourseVersions.ToDictionary(version => version.CourseVersionID);

            foreach (var versionDto in courseDetail.CourseVersions)
            {
                if (!versionEntitiesById.TryGetValue(versionDto.CourseVersionID, out var versionEntity))
                    continue;

                if (versionEntity.UpdateBy is Guid updaterId && usersById.TryGetValue(updaterId, out var updater))
                {
                    versionDto.Updater = new SimpleUserReponse
                    {
                        UserId = updater.UserId,
                        FullName = AppHelper.GetFullName(updater) ?? updater.Username,
                        Email = updater.Email,
                        AvatarUrl = updater.ImageUrl
                    };
                }
            }
        }

        return courseDetail;
    }
}

