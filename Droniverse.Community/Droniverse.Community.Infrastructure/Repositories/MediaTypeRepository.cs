using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class MediaTypeRepository : MySqlRepository<MediaType>, IMediaTypeRepository
{
    public MediaTypeRepository(MySqlDbContext context) : base(context)
    {
    }
}

