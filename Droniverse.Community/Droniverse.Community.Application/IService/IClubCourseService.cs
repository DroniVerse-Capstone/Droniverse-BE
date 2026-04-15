using Droniverse.Community.Application.DTO.Request;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.IService
{
    public interface IClubCourseService
    {
        Task<ClubCourseResponseDto> AddCourseToClub(Guid clubId, AddClubCourseRequest request);
        Task<ClubCourseResponseDto> UpdateClubCourse(Guid clubId, Guid courseId, UpdateClubCourseRequest request);
        Task<ClubCourseResponseDto> IncreaseCapacity(Guid clubId, Guid courseId, IncreaseClubCourseCapacityRequest request);
        Task<ClubCourseResponseDto> ConsumeSlot(Guid clubId, Guid courseId, ChangeClubCourseSlotRequest? request);
        Task<ClubCourseResponseDto> RestoreSlot(Guid clubId, Guid courseId, ChangeClubCourseSlotRequest? request);
        Task<ClubCourseRemainingQuantityResponseDto> GetRemainingQuantity(Guid clubId, Guid courseId);
    }
}
