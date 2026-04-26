using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class AssignmentRepository : MySqlRepository<Assignment>, IAssignmentRepository
{
    public AssignmentRepository(MySqlDbContext context) : base(context)
    {
    }
}
