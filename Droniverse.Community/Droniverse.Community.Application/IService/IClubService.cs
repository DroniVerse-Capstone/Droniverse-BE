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
public interface IClubService
{
    Task<Guid> GetDroneFromClub(Guid clubId);
    Task<PaginationResult<IEnumerable<ClubResponseDto>>> GetAllClubs(GetAllClubsSearchRequest request);
    Task<ClubResponseDto> GetClubById(Guid id);
    Task<ClubResponseDto> GetClubByClubCode(string clubCode);
    Task<ClubResponseDto> CreateClub(ClubCreateDto club);
    Task<ClubResponseDto> UpdateClub(Guid id, ClubUpdateDto club);
    Task<bool> DeleteClub(Guid id);
    Task<JoinClubResponse> JoinClub(ClubJoinDto request);
    Task<PaginationResult<IEnumerable<GetParticipantsResponse>>> GetClubParcitipations(Guid clubID, ParticipationSearchRequest searchRequest);
    //Task<PaginationResult<IEnumerable<CourseBulkResponseDTO>>> GetHotCoursesByClub(Guid clubId, HotCoursesSearchRequest searchRequest);
    Task<IEnumerable<ClubResponseDto>> GetClubsByCurrentUsersID(ClubStatus? status = null);
    Task<IEnumerable<SimpleClubResponse>> GetClubInfoBulk(GetClubSimpleInfoRequest request);
    Task<ClubResponseDto> UpdateClubStatus(Guid clubId, ClubUpdateStatusDto dto);
    Task<GetClubParticipantsResponse> GetClubParticipantIds(Guid clubId, GetClubParticipantIdsRequest request);
    Task<bool> CheckParticipant(Guid clubId, Guid userId, ParticipationStatus status = ParticipationStatus.ACTIVE);
}

