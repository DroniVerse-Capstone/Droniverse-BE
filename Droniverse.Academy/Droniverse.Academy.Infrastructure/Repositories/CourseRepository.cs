using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CourseRepository : MySqlRepository<Course>, ICourseRepository
{
    public CourseRepository(MySqlDbContext context) : base(context)
    {
    }
}

