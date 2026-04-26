using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class UserAssignmentRepository : MySqlRepository<UserAssignment>, IUserAssignmentRepository
{
    public UserAssignmentRepository(MySqlDbContext context) : base(context)
    {
    }
}
