using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.IService;
public interface IClubService
{
    Task<PaginationResult<IEnumerable<ClubResponseDto>>> GetAllClubs(GetAllClubsSearchRequest request);
    Task<ClubResponseDto> GetClubById(Guid id);
    Task<ClubResponseDto> GetClubByClubCode(string clubCode);
    Task<ClubResponseDto> CreateClub(ClubCreateDto club);
    Task<ClubResponseDto> UpdateClub(Guid id, ClubUpdateDto club);
    Task<bool> DeleteClub(Guid id);
    Task<JoinClubResponse> JoinClub(ClubJoinDto request);
    Task<PaginationResult<IEnumerable<UserResponse>>> GetClubParcitipations(Guid clubID, ParticipationSearchRequest searchRequest);
    Task<PaginationResult<IEnumerable<CourseBulkResponseDTO>>> GetClubCourses(Guid clubId, CourseBulkSearchRequest searchRequest);
    Task<IEnumerable<ClubResponseDto>> GetClubsByCurrentUsersID(ClubStatus? status = null);
    
    // ===== Status Management Methods =====
    /// <summary>
    /// Update Club Status với phân quyền động
    /// </summary>
    Task<ClubResponseDto> UpdateClubStatus(Guid clubId, ClubUpdateStatusDto dto);
    
    /// <summary>
    /// Suspend club (ADMIN, SYSTEM_MANAGER only)
    /// </summary>
    [Obsolete("Use UpdateClubStatus instead")]
    Task<ClubResponseDto> SuspendClub(Guid clubId, string? reason = null);
    
    /// <summary>
    /// Archive club - đóng hẳn (ADMIN, CLUB_MANAGER, SYSTEM_MANAGER)
    /// </summary>
    [Obsolete("Use UpdateClubStatus instead")]
    Task<ClubResponseDto> ArchiveClub(Guid clubId, string? reason = null);
    
    /// <summary>
    /// Restore club from SUSPENDED or INACTIVE to ACTIVE
    /// </summary>
    [Obsolete("Use UpdateClubStatus instead")]
    Task<ClubResponseDto> RestoreClub(Guid clubId);
    
    /// <summary>
    /// Deactivate club - CLUB_MANAGER can mark as INACTIVE
    /// </summary>
    [Obsolete("Use UpdateClubStatus instead")]
    Task<ClubResponseDto> DeactivateClub(Guid clubId);
}

