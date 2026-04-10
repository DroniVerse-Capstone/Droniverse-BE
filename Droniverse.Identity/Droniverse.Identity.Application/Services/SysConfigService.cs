using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Application.Services
{
    public class SysConfigService : ISysConfigService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SysConfigService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<CertificateTemplateResponse> GetCertificateTemplate()
        {
            var certificateTemplate = await _unitOfWork.SysConfigs.GetCertificateTemplate();
            return new CertificateTemplateResponse { ImageUrl = certificateTemplate.ImageUrl };
        }
    }
}
