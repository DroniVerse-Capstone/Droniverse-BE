using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CourseVersionRepository : MySqlRepository<CourseVersion>, ICourseVersionRepository
{
    public CourseVersionRepository(MySqlDbContext context) : base(context)
    {
    }
}

