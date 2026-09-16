using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Droniverse.Identity.Application.IService;

public interface ISysPolicyService
{
    Task<PaginationResult<IEnumerable<SysPolicyResponse>>> GetAllSysPolicies(SysPolicySearchRequest searchRequest, int pageIndex, int pageSize);
    Task<SysPolicyResponse> GetSysPolicyById(Guid id);
    Task<SysPolicyResponse> AddSysPolicy(SysPolicyCreateDto dto);
    Task<SysPolicyResponse> UpdateSysPolicy(Guid id, SysPolicyUpdateDto dto);
    Task<bool> DeleteSysPolicy(Guid id);
}
