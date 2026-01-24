using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class ModuleRepository : MySqlRepository<Module>, IModuleRepository
{
    public ModuleRepository(MySqlDbContext context) : base(context)
    {
    }
}

