using Droniverse.Community.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.IRepository
{
    public interface IClubCourseRepository : IRepository<ClubCourse>
    {
        Task<int> CountCoursesByClubIdAsync(Guid clubId);
        Task<bool> ExistsAsync(Guid clubId, Guid courseId);
        Task<ClubCourse?> GetByClubAndCourseAsync(Guid clubId, Guid courseId, bool asNoTracking = false);
        //Task<bool> CanGenerate(Guid clubId, Guid courseId, int quantity);

    }
}
