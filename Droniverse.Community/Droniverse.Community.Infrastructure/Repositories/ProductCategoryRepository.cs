using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ProductCategoryRepository : MySqlRepository<ProductCategory>, IProductCategoryRepository
{
    public ProductCategoryRepository(MySqlDbContext context) : base(context)
    {
    }
}

