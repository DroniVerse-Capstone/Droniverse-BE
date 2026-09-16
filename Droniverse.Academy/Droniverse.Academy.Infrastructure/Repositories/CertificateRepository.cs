using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CertificateRepository : MySqlRepository<Certificate>, ICertificateRepository
{
    public CertificateRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SimpleCertificateResponse>> GetSimpleCertificatesByIdsAsync(IEnumerable<Guid> certificateIds, CancellationToken cancellationToken = default)
    {
        var ids = certificateIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
            return [];

        return await _dbSet
            .AsNoTracking()
            .Where(x => ids.Contains(x.CertificateID))
            .Select(x => new SimpleCertificateResponse
            {
                CertificateID = x.CertificateID,
                CertificateNameVN = x.CertificateNameVN,
                CertificateNameEN = x.CertificateNameEN,
                ImageUrl = x.ImageUrl
            })
            .ToListAsync(cancellationToken);
    }
}

