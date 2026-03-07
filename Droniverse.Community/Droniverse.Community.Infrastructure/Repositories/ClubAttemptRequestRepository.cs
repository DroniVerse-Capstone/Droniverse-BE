using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ClubAttemptRequestRepository : MySqlRepository<ClubAttemptRequest>, IClubAttemptRequestRepository
{
    public ClubAttemptRequestRepository(MySqlDbContext context) : base(context)
    {

    }

    public async Task<bool> IsUserInClubAttemptRequest(Guid userID, Guid clubID)
    {
        return await _context.Set<ClubAttemptRequest>().AnyAsync(x => x.RequesterID == userID && x.ClubID == clubID && x.Status == Domain.Enums.ClubAttemptRequestStatus.PENDING);
    }
}

