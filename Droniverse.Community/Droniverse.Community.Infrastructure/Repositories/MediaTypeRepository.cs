using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class MediaTypeRepository : Repository<MediaType>, IMediaTypeRepository
{
    public MediaTypeRepository(MySqlDbContext context) : base(context)
    {
    }
}

