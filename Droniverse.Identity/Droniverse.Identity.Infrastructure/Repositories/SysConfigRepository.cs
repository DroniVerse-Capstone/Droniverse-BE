using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Domain.QueryModels;
using Droniverse.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Infrastructure.Repositories
{
    public class SysConfigRepository : Repository<SysConfig>, ISysConfigRepository
    {

        public SysConfigRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<CertificateTemplateQueryModel> GetCertificateTemplate()
        {
            return await _context.SysConfig
                .AsNoTracking()
                .Select(s => new CertificateTemplateQueryModel
                {
                    ImageUrl = s.CertificateTemplateUrl
                })
                .FirstAsync();
        }
        public async Task<SystemEstimateTimeQueryModel> GetSystemEstimateTime()
        {
            return await _context.SysConfig
                .AsNoTracking()
                .Select(s => new SystemEstimateTimeQueryModel
                {
                    BufferEstimatedDuration = s.BufferEstimatedDuration
                })
                .FirstAsync();
        }
    }
    }
