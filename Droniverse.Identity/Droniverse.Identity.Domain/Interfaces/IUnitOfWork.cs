using Droniverse.Identity.Domain.Entities;

namespace Droniverse.Identity.Domain.Interfaces;
public interface IUnitOfWork : IDisposable
{
    IRoleRepository Roles { get; }
    IUserRepository Accounts { get; }
    IUserInfoRepository UserInfos { get; }
    IPermissionRepository Permissions { get; }
    ISysConfigRepository SysConfigs { get; }
    INotificationRepository Notifications { get; }
    Task<int> SaveChangeAsync();
}
