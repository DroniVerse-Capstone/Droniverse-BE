using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Infrastructure.Repositories
{
    public class ClubCreationRequestRepository : MySqlRepository<ClubCreationRequest>, IClubCreationRequestRepository
    {
        public ClubCreationRequestRepository(MySqlDbContext context) : base(context) { }

        public async Task<bool> IsUserHavingOtherRequest(Guid userID)
        {
            return await _context.Set<ClubCreationRequest>().AnyAsync(c => c.RequesterID == userID && c.Status == Domain.Enums.ClubCreationRequestStatus.PENDING);
        }
    }
}
