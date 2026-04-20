using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;

namespace Droniverse.Community.Application.IService;
public interface IClubPolicyService
{
    Task<PaginationResult<IEnumerable<ClubPolicyResponseDto>>> GetAllClubPolicies(GetAllClubPoliciesSearchRequest request);
    Task<ClubPolicyResponseDto> CreateClubPolicyAsync(ClubPolicyCreateDto request);
}

