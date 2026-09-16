using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Domain.Interfaces
{
    public interface IUserInfoRepository : IRepository<UserInfo>
    {
        Task<(int TotalItems, List<SimpleUserReponse> Users)> SearchUsersWithPaginationAsync(
                 string? fullName,
                 string? email,
                 List<Guid>? userIds, 
                 int pageIndex,
                 int pageSize);
    }
}
