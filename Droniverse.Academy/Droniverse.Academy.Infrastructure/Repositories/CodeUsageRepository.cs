using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CodeUsageRepository : MySqlRepository<CodeUsage>, ICodeUsageRepository
{
    public CodeUsageRepository(MySqlDbContext context) : base(context)
    {
    }
}

