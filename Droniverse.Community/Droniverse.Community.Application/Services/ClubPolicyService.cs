using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Academy.Application.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using Droniverse.Shared.Constants;
using Droniverse.Shared.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Community.Application.Services;
internal class ClubPolicyService : IClubPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly AcademyMicroserviceClient _academyMicroserviceClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClock _clock;

    public ClubPolicyService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IdentityMicroserviceClient identityMicroserviceClient,
        AcademyMicroserviceClient academyMicroserviceClient,
        ICurrentUserService currentUserService,
        IClock clock
        )
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _identityMicroserviceClient = identityMicroserviceClient;
        _academyMicroserviceClient = academyMicroserviceClient;
        _currentUserService = currentUserService;
        _clock = clock;
    }



 
    public async Task<PaginationResult<IEnumerable<ClubPolicyResponseDto>>> GetAllClubPolicies(GetAllClubPoliciesSearchRequest request)
    {
        request ??= new GetAllClubPoliciesSearchRequest();

        var currentPage = request.CurrentPage < 1 ? 1 : request.CurrentPage;
        var pageSize = request.PageSize < 5 ? 5 : (request.PageSize > 20 ? 20 : request.PageSize);

        var clubResult = await _unitOfWork.ClubPolicies.GetAll(
                   currentPage,
                   pageSize);

        var clubPolicyList = clubResult.Data?.ToList() ?? [];
        if (clubPolicyList.Count == 0)
            return new PaginationResult<IEnumerable<ClubPolicyResponseDto>>([], clubResult.TotalRecords, currentPage, pageSize);
        var mappedClubPolicies = _mapper.Map<IEnumerable<ClubPolicyResponseDto>>(clubPolicyList);
        return new PaginationResult<IEnumerable<ClubPolicyResponseDto>>(
            mappedClubPolicies,
            clubResult.TotalRecords,
            currentPage,
            pageSize
        );
    }

    public async Task<ClubPolicyResponseDto> CreateClubPolicyAsync(ClubPolicyCreateDto request)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == request.ClubId);
        if (club == null)
            throw new NotFoundException($"Không tìm thấy club {request.ClubId}!");

        Guid userID = _currentUserService.UserId;
        if(userID == Guid.Empty)
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực!");

        ClubPolicy clubPolicy = _mapper.Map<ClubPolicy>(request);
        clubPolicy.CreatedAt = _clock.Now;
        clubPolicy.CreatedBy = userID;
        await _unitOfWork.ClubPolicies.Add(clubPolicy);
        await _unitOfWork.SaveChangeAsync();
        return _mapper.Map<ClubPolicyResponseDto>(clubPolicy);
    }
}

