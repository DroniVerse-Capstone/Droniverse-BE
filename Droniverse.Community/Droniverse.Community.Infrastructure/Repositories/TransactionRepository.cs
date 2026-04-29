using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;

internal class TransactionRepository : MySqlRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByWalletIdAsync(Guid walletId)
    {
        return await _dbSet
            .Where(t => t.WalletID == walletId)
            .Include(t => t.Wallet)
            .Include(t => t.Club)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}

