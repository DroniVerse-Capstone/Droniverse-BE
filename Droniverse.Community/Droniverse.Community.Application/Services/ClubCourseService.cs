using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class ClubCourseService : IClubCourseService
    {

        private readonly IUnitOfWork _unitOfWork;

        public ClubCourseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ClubCourseResponseDto> AddCourseToClub(Guid clubId, AddClubCourseRequest request)
        {
            if (clubId == Guid.Empty)
                throw new ArgumentException("ClubId không hợp lệ.", nameof(clubId));

            if (request == null)
                throw new ArgumentNullException(nameof(request), "Dữ liệu thêm khóa học không được để trống.");

            if (request.CourseId == Guid.Empty)
                throw new ArgumentException("CourseId không hợp lệ.", nameof(request.CourseId));

            var club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId, q => q.AsNoTracking());
            if (club == null)
                throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

            var existed = await _unitOfWork.ClubCourses.ExistsAsync(clubId, request.CourseId);

            if (existed)
                throw new InvalidOperationException("Khóa học đã tồn tại trong câu lạc bộ.");

            var clubCourse = ClubCourse.Create(
                clubId,
                request.CourseId,
                request.TotalQuantity,
                request.ProfitType);

            await _unitOfWork.ClubCourses.Add(clubCourse);
            await _unitOfWork.SaveChangeAsync();

            return ToResponse(clubCourse);
        }

        public async Task<ClubCourseResponseDto> UpdateClubCourse(Guid clubId, Guid courseId, UpdateClubCourseRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Dữ liệu cập nhật không được để trống.");

            var clubCourse = await GetExistingClubCourse(clubId, courseId);

            if (request.TotalQuantity.HasValue)
                clubCourse.UpdateTotalQuantity(request.TotalQuantity.Value);

            if (request.ProfitType.HasValue)
                clubCourse.UpdateProfitType(request.ProfitType.Value);

            await _unitOfWork.SaveChangeAsync();
            return ToResponse(clubCourse);
        }

        public async Task<ClubCourseResponseDto> IncreaseCapacity(Guid clubId, Guid courseId, IncreaseClubCourseCapacityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Dữ liệu tăng slot không được để trống.");

            var clubCourse = await GetExistingClubCourse(clubId, courseId);
            clubCourse.IncreaseCapacity(request.Quantity);

            await _unitOfWork.SaveChangeAsync();
            return ToResponse(clubCourse);
        }

        public async Task<ClubCourseResponseDto> ConsumeSlot(Guid clubId, Guid courseId, ChangeClubCourseSlotRequest? request)
        {
            var quantity = request?.Quantity ?? 1;

            var clubCourse = await GetExistingClubCourse(clubId, courseId);
            clubCourse.Consume(quantity);

            await _unitOfWork.SaveChangeAsync();
            return ToResponse(clubCourse);
        }

        public async Task<ClubCourseResponseDto> RestoreSlot(Guid clubId, Guid courseId, ChangeClubCourseSlotRequest? request)
        {
            var quantity = request?.Quantity ?? 1;

            var clubCourse = await GetExistingClubCourse(clubId, courseId);
            clubCourse.Restore(quantity);

            await _unitOfWork.SaveChangeAsync();
            return ToResponse(clubCourse);
        }

        private async Task<ClubCourse> GetExistingClubCourse(Guid clubId, Guid courseId)
        {
            if (clubId == Guid.Empty)
                throw new ArgumentException("ClubId không hợp lệ.", nameof(clubId));

            if (courseId == Guid.Empty)
                throw new ArgumentException("CourseId không hợp lệ.", nameof(courseId));

            var clubCourse = await _unitOfWork.ClubCourses.GetByClubAndCourseAsync(clubId, courseId);
            if (clubCourse == null)
                throw new KeyNotFoundException($"Không tìm thấy khóa học [{courseId}] trong câu lạc bộ [{clubId}].");

            return clubCourse;
        }

        private static ClubCourseResponseDto ToResponse(ClubCourse clubCourse)
        {
            return new ClubCourseResponseDto
            {
                ClubId = clubCourse.ClubID,
                CourseId = clubCourse.CourseID,
                TotalQuantity = clubCourse.TotalQuantity,
                RemainingQuantity = clubCourse.RemainingQuantity,
                ProfitType = clubCourse.ProfitType,
                IsAvailable = clubCourse.IsAvailable()
            };
        }
    }
}
