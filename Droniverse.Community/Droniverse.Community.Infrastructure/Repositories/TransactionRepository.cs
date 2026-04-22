using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class TransactionRepository : MySqlRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(MySqlDbContext context) : base(context)
    {
    }

}

