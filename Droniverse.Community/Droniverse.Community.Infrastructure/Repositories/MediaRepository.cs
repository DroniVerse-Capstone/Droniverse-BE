using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class MediaRepository : Repository<Media>, IMediaRepository
{
    public MediaRepository(MySqlDbContext context) : base(context)
    {
    }
}

