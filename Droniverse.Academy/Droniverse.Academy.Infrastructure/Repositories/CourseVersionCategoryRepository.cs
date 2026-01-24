using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CourseVersionCategoryRepository : MySqlRepository<CourseVersionCategory>, ICourseVersionCategoryRepository
{
    public CourseVersionCategoryRepository(MySqlDbContext context) : base(context)
    {
    }
}

