using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CodeRepository : MySqlRepository<Code>, ICodeRepository
{
    public CodeRepository(MySqlDbContext context) : base(context)
    {
    }
}

