using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;

namespace Droniverse.Identity.API.Controllers
{
    [ApiController]
    [Route("identity/system-configs")]
    [Authorize]
    public class SysConfigController : ControllerBase
    {
        private readonly ISysConfigService _sysConfigService;

        public SysConfigController(ISysConfigService sysConfigService)
        {
            _sysConfigService = sysConfigService;
        }

        [HttpGet("certificate")]
        public async Task<ActionResult<CertificateTemplateResponse>> GetCertificateTemplate()
        {
            var result = await _sysConfigService.GetCertificateTemplate();
            return Ok(result);
        }
    }
}
