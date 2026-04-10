using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Domain.Interfaces
{
    public interface ISysConfigRepository : IRepository<SysConfig>
    {
        Task<CertificateTemplateQueryModel> GetCertificateTemplate();
    }
}
