using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;

namespace Droniverse.Identity.Infrastructure.Repositories;
internal class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(IdentityDbContext context) : base(context)
    {
    }
}