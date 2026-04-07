using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.IService
{
    public interface IClubCourseService
    {
        Task<ClubCourseResponseDto> AddCourseToClub(Guid clubId, AddClubCourseRequest request);
        Task<ClubCourseResponseDto> UpdateClubCourse(Guid clubId, Guid courseId, UpdateClubCourseRequest request);
        Task<ClubCourseResponseDto> IncreaseCapacity(Guid clubId, Guid courseId, IncreaseClubCourseCapacityRequest request);
        Task<ClubCourseResponseDto> ConsumeSlot(Guid clubId, Guid courseId, ChangeClubCourseSlotRequest? request);
        Task<ClubCourseResponseDto> RestoreSlot(Guid clubId, Guid courseId, ChangeClubCourseSlotRequest? request);
    }
}
