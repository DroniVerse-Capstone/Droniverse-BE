using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CourseRepository : MySqlRepository<Course>, ICourseRepository
{
    public CourseRepository(MySqlDbContext context) : base(context)
    {
        
    }
    
    public override async Task<Course?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await base._dbSet
                .Where(c => c.CourseID == (Guid)id)
                .Include(c => c.CourseVersions)
                .Include(c => c.Certificate)
                .FirstOrDefaultAsync(cancellationToken);
    }
}

