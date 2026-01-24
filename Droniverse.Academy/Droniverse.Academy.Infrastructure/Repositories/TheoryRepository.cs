using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class TheoryRepository : MySqlRepository<Theory>, ITheoryRepository
{
    public TheoryRepository(MySqlDbContext context) : base(context)
    {
    }
}

