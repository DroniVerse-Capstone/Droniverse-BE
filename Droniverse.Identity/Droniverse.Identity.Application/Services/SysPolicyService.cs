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
            p.Title,
            p.Content,
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
            policy.Title,
            policy.Content,
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
            Title = dto.Title,
            Content = dto.Content,
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
            entity.Title,
            entity.Content,
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
        existing.Title = dto.Title;
        existing.Content = dto.Content;
        existing.EffectiveDate = dto.EffectiveDate;
        existing.UpdatedAt = _clock.Now;
        existing.UpdatedBy = _currentUserService.UserId;

        await _unitOfWork.SysPolicies.Update(existing);
        await _unitOfWork.SaveChangeAsync();

        return new SysPolicyResponse(
            existing.SysPolicyID,
            existing.Type,
            existing.Title,
            existing.Content,
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
