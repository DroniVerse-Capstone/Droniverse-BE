using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.IService;
public interface IClubService
{
    Task<IEnumerable<ClubResponseDto>> GetAllClubs();
    Task<ClubResponseDto> GetClubById(Guid id);
    Task<ClubResponseDto> CreateClub(ClubCreateDto club);
    Task<ClubResponseDto> UpdateClub(Guid id, ClubUpdateDto club);
    Task<bool> DeleteClub(Guid id);
    Task<JoinClubResponse> JoinClub(ClubJoinDto request);
    Task<IEnumerable<CourseResponseDto>> GetClubCourses(Guid clubId, ClubCourseSearchRequest searchRequest);
}

