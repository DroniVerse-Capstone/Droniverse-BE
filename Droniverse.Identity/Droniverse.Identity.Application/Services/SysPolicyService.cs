using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Droniverse.Identity.Application.Services;

internal class SysPolicyService : ISysPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClock _clock;

    public SysPolicyService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _clock = clock;
    }

    public async Task<PaginationResult<IEnumerable<SysPolicyResponse>>> GetAllSysPolicies(SysPolicySearchRequest searchRequest, int pageIndex, int pageSize)
    {
        searchRequest ??= new SysPolicySearchRequest();
        var paged = await _unitOfWork.SysPolicies.GetAllSysPoliciesAsync(searchRequest, searchRequest.CurrentPage, searchRequest.PageSize);

        var mapped = paged.Data.Select(p => new SysPolicyResponse(
            p.SysPolicyID,
            p.Type,
            p.TitleEN,
            p.TitleVN,
            p.ContentEN,
            p.ContentVN,
            p.EffectiveDate,
            p.CreatedAt,
            p.CreatedBy
        ));

        return new PaginationResult<IEnumerable<SysPolicyResponse>>(mapped, paged.TotalRecords, paged.PageIndex, paged.PageSize);
    }

    public async Task<SysPolicyResponse> GetSysPolicyById(Guid id)
    {
        var policy = await _unitOfWork.SysPolicies.GetByCondition(p => p.SysPolicyID == id);
        if (policy == null)
            throw new Exception("SysPolicy not found.");

        return new SysPolicyResponse(
            policy.SysPolicyID,
            policy.Type,
            policy.TitleEN,
            policy.TitleVN,
            policy.ContentEN,
            policy.ContentVN,
            policy.EffectiveDate,
            policy.CreatedAt,
            policy.CreatedBy
        );
    }

    public async Task<SysPolicyResponse> AddSysPolicy(SysPolicyCreateDto dto)
    {
        var now = _clock.Now;
        var userId = _currentUserService.UserId;

        var entity = new SysPolicy
        {
            SysPolicyID = Guid.NewGuid(),
            Type = dto.Type,
            TitleEN = dto.TitleEN,
            TitleVN = dto.TitleVN,
            ContentEN = dto.ContentEN,
            ContentVN = dto.ContentVN,
            EffectiveDate = dto.EffectiveDate,
            CreatedAt = now,
            CreatedBy = userId,
            UpdatedAt = now,
            UpdatedBy = userId
        };

        await _unitOfWork.SysPolicies.Add(entity);
        await _unitOfWork.SaveChangeAsync();

        return new SysPolicyResponse(
            entity.SysPolicyID,
            entity.Type,
            entity.TitleEN,
            entity.TitleVN,
            entity.ContentEN,
            entity.ContentVN,
            entity.EffectiveDate,
            entity.CreatedAt,
            entity.CreatedBy
        );
    }

    public async Task<SysPolicyResponse> UpdateSysPolicy(Guid id, SysPolicyUpdateDto dto)
    {
        var existing = await _unitOfWork.SysPolicies.GetByCondition(p => p.SysPolicyID == id);
        if (existing == null)
            throw new Exception("SysPolicy not found.");

        existing.Type = dto.Type;
        existing.TitleEN = dto.TitleEN;
        existing.TitleVN = dto.TitleVN;
        existing.ContentEN = dto.ContentEN;
        existing.ContentVN = dto.ContentVN;
        existing.EffectiveDate = dto.EffectiveDate;
        existing.UpdatedAt = _clock.Now;
        existing.UpdatedBy = _currentUserService.UserId;

        await _unitOfWork.SysPolicies.Update(existing);
        await _unitOfWork.SaveChangeAsync();

        return new SysPolicyResponse(
            existing.SysPolicyID,
            existing.Type,
            existing.TitleEN,
            existing.TitleVN,
            existing.ContentEN,
            existing.ContentVN,
            existing.EffectiveDate,
            existing.CreatedAt,
            existing.CreatedBy
        );
    }

    public async Task<bool> DeleteSysPolicy(Guid id)
    {
        var existing = await _unitOfWork.SysPolicies.GetByCondition(p => p.SysPolicyID == id);
        if (existing == null)
            throw new Exception("SysPolicy not found.");

        await _unitOfWork.SysPolicies.Delete(existing);
        await _unitOfWork.SaveChangeAsync();
        return true;
    }
}
