using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Application.IService
{
    public interface ISysConfigService
    {
        Task<CertificateTemplateResponse> GetCertificateTemplate();
    }
}
