using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ProductRepository : MySqlRepository<Product>, IProductRepository
{
    public ProductRepository(MySqlDbContext context) : base(context)
    {
    }
}

