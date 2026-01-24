using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class UserAttemptRepository : MySqlRepository<UserAttempt>, IUserAttemptRepository
{
    public UserAttemptRepository(MySqlDbContext context) : base(context)
    {
    }
}

