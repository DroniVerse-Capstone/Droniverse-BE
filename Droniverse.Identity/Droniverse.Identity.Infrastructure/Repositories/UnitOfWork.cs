using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;

namespace Droniverse.Identity.Infrastructure.Repositories;
internal class UnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _context;
    private IRoleRepository _role;
    private IUserRepository _account;
    private IRepository<UserInfo> _userInfo;
    private IPermissionRepository _permission;

    public UnitOfWork(IdentityDbContext context)
    {
        _context = context;
    }

    public IRoleRepository Roles => _role ??= new RoleRepository(_context);

    public IUserRepository Accounts => _account ??= new UserRepository(_context);

    public IRepository<UserInfo> UserInfos => _userInfo ??= new Repository<UserInfo>(_context);

    public IPermissionRepository Permissions => _permission ??= new PermissionRepository(_context);

    public void Dispose() // dùng để đóng kết nối với DbContext
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<int> SaveChangeAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

