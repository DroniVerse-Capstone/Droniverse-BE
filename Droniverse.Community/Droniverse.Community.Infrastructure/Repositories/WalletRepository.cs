using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class WalletRepository : MySqlRepository<Wallet>, IWalletRepository
{
    public WalletRepository(MySqlDbContext context) : base(context)
    {
    }

}

