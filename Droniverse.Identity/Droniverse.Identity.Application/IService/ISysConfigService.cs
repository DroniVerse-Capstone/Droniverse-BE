using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
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
        Task<SystemEstimatetime> GetSystemEstimateTime();
    }
}
