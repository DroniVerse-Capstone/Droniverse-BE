using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class EnrollmentRepository : MySqlRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(MySqlDbContext context) : base(context)
    {
    }
}

