using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(MySqlDbContext context) : base(context)
    {
    }
}

