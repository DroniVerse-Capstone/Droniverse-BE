using Droniverse.Community.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.IRepository
{
    public interface IUserPrizeRepository : IRepository<UserPrize>
    {
        Task AddRange(IEnumerable<UserPrize> entities);

    }
}
