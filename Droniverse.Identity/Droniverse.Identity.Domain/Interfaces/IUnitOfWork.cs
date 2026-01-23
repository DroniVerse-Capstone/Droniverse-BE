using Droniverse.Identity.Domain.Entities;

namespace Droniverse.Identity.Domain.Interfaces;
public interface IUnitOfWork : IDisposable
{
    IRepository<Role> Roles { get; }
    IRepository<Account> Accounts { get; }
    IRepository<UserInfo> UserInfos { get; }
    IRepository<Permission> Permissions { get; }

    Task<int> SaveChangeAsync();
}
