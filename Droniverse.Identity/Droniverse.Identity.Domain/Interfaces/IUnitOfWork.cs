using Droniverse.Identity.Domain.Entities;

namespace Droniverse.Identity.Domain.Interfaces;
public interface IUnitOfWork : IDisposable
{
    IRoleRepository Roles { get; }
    IUserRepository Accounts { get; }
    IRepository<UserInfo> UserInfos { get; }
    IPermissionRepository Permissions { get; }

    Task<int> SaveChangeAsync();
}
