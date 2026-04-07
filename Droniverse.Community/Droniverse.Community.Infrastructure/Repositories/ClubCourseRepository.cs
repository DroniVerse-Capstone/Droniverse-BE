using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Infrastructure.Repositories
{
    public class ClubCourseRepository : MySqlRepository<ClubCourse>, IClubCourseRepository
    {
        public ClubCourseRepository(MySqlDbContext context) : base(context) { }

        public async Task<int> CountCoursesByClubIdAsync(Guid clubId)
        {
            return await _dbSet
                .Where(cc => cc.ClubID == clubId)
                .CountAsync();
        }

        public async Task<bool> ExistsAsync(Guid clubId, Guid courseId)
        {
            return await _dbSet.AnyAsync(cc => cc.ClubID == clubId && cc.CourseID == courseId);
        }

        public async Task<ClubCourse?> GetByClubAndCourseAsync(Guid clubId, Guid courseId, bool asNoTracking = false)
        {
            IQueryable<ClubCourse> query = _dbSet;

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cc => cc.ClubID == clubId && cc.CourseID == courseId);
        }
    }
}
