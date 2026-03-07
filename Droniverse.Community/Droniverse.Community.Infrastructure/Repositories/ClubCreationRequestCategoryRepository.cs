using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Infrastructure.Repositories;

public class ClubCreationRequestCategoryRepository : MySqlRepository<ClubCreationRequestCategory>, IClubCreationRequestCategoryRepository
{

    public ClubCreationRequestCategoryRepository(MySqlDbContext context) : base(context)
    {

    }
}

