using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class WithdrawRequestRepository : MySqlRepository<WithdrawRequest>, IWithdrawRequestRepository
{
    public WithdrawRequestRepository(MySqlDbContext context) : base(context)
    {
    }

}

