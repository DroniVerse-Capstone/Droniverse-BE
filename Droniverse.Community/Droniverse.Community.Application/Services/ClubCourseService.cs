using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Services
{
    public class ClubCourseService : IClubCourseService
    {

        private readonly IUnitOfWork _unitOfWork;

        public ClubCourseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddCourseToClub(Guid clubId, Guid courseId, AddClubCourseRequest request)
        {
            //if (clubId == Guid.Empty)
            //    throw new ArgumentException("ClubId không hợp lệ.", nameof(clubId));

            //if (courseId == Guid.Empty)
            //    throw new ArgumentException("CourseId không hợp lệ.", nameof(courseId));

            //var club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId);
            //if (club == null)
            //    throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

            //var existed = await _unitOfWork.ClubCourses.GetByCondition(
            //    cc => cc.ClubID == clubId && cc.CourseID == courseId);

            //if (existed == null)
            //{
            //    existed.
            //}
            //else
            //{

            //}

            //var clubCourse = new ClubCourse
            //{
            //    ClubID = clubId,
            //    CourseID = courseId,
            //    isProfit = ClubCourseProfit.PROFIT
            //};

            //await _unitOfWork.ClubCourses.Add(clubCourse);
            //await _unitOfWork.SaveChangeAsync();

            return true;
        }
    }
}
