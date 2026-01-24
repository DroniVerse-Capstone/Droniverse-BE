using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class ReportRepository : MySqlRepository<Report>, IReportRepository
{
    public ReportRepository(MySqlDbContext context) : base(context)
    {
    }
}

