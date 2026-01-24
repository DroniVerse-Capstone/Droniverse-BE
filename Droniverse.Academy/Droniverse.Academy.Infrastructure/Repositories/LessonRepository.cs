using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class LessonRepository : MySqlRepository<Lesson>, ILessonRepository
{
    public LessonRepository(MySqlDbContext context) : base(context)
    {
    }
}

