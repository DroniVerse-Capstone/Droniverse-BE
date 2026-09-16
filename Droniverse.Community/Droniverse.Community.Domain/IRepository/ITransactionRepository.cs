using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Domain.IRepository;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetTransactionsByWalletIdAsync(Guid walletId);
}



