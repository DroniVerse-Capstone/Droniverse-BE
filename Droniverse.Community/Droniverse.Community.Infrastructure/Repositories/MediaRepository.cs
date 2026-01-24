using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class MediaRepository : MySqlRepository<Media>, IMediaRepository
{
    public MediaRepository(MySqlDbContext context) : base(context)
    {
    }
}

