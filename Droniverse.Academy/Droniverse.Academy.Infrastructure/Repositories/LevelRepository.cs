using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Infrastructure.Repositories
{
    internal class LevelRepository : MySqlRepository<Level>, ILevelRepository
    {
        public LevelRepository(MySqlDbContext context) : base(context)
        {
        }
    }
}
