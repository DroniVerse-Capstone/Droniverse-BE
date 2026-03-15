using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Abstractions;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class ModuleService : IModuleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IClock _clock;

    public ModuleService(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _clock = clock;
    }

    public async Task<ModuleClientViewDTO> CreateModuleAsync(Guid courseId, Guid versionId, CreateModuleRequestDTO request)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);
        await ValidateModuleNumberAsync(versionId, request.ModuleNumber);

        var module = new Module
        {
            ModuleID = Guid.NewGuid(),
            CourseVersionID = versionId,
            TitleVN = request.TitleVN,
            TitleEN = request.TitleEN,
            ModuleNumber = request.ModuleNumber,
            CreateAt = _clock.Now,
            UpdateAt = _clock.Now
        };

        await _unitOfWork.Modules.AddAsync(module);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ModuleClientViewDTO>(module);
    }

    public async Task<IEnumerable<ModuleClientViewDTO>> GetModulesAsync(Guid courseId, Guid versionId)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var modules = await _unitOfWork.Modules.GetAllAsync(
            filter: m => m.CourseVersionID == versionId,
            orderBy: q => q.OrderBy(m => m.ModuleNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return _mapper.Map<IEnumerable<ModuleClientViewDTO>>(modules.Data);
    }

    public async Task<ModuleClientViewDTO> GetModuleByIdAsync(Guid courseId, Guid versionId, Guid moduleId)
    {
        var module = await GetModuleEntityAsync(courseId, versionId, moduleId);
        return _mapper.Map<ModuleClientViewDTO>(module);
    }

    public async Task<ModuleClientViewDTO> UpdateModuleAsync(Guid courseId, Guid versionId, Guid moduleId, UpdateModuleRequestDTO request)
    {
        var module = await GetModuleEntityAsync(courseId, versionId, moduleId);
        await ValidateModuleNumberAsync(versionId, request.ModuleNumber, moduleId);

        module.TitleVN = request.TitleVN;
        module.TitleEN = request.TitleEN;
        module.ModuleNumber = request.ModuleNumber;
        module.UpdateAt = _clock.Now;

        await _unitOfWork.Modules.UpdateAsync(module);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ModuleClientViewDTO>(module);
    }

    public async Task DeleteModuleAsync(Guid courseId, Guid versionId, Guid moduleId)
    {
        var module = await GetModuleEntityAsync(courseId, versionId, moduleId);

        await _unitOfWork.Modules.DeleteAsync(module);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ModuleClientViewDTO>> ReorderModulesAsync(Guid courseId, Guid versionId, ReorderModulesRequestDTO request)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        if (request.Modules.Count == 0)
            throw new ValidationException("Modules reorder payload is required.");

        if (request.Modules.Select(x => x.ModuleID).Distinct().Count() != request.Modules.Count)
            throw new ValidationException("Duplicate moduleId in reorder payload.");

        if (request.Modules.Select(x => x.ModuleNumber).Distinct().Count() != request.Modules.Count)
            throw new ValidationException("moduleNumber must be unique in reorder payload.");

        if (request.Modules.Any(x => x.ModuleNumber <= 0))
            throw new ValidationException("moduleNumber must be greater than 0.");

        var modulesResult = await _unitOfWork.Modules.GetAllAsync(
            filter: m => m.CourseVersionID == versionId,
            orderBy: q => q.OrderBy(m => m.ModuleNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var modules = modulesResult.Data.ToList();
        if (modules.Count != request.Modules.Count)
            throw new ValidationException("Reorder payload must contain all modules of the course version.");

        var moduleIds = modules.Select(m => m.ModuleID).OrderBy(x => x).ToList();
        var requestIds = request.Modules.Select(m => m.ModuleID).OrderBy(x => x).ToList();
        if (!moduleIds.SequenceEqual(requestIds))
            throw new ValidationException("Reorder payload contains invalid moduleId.");

        var reorderMap = request.Modules.ToDictionary(x => x.ModuleID, x => x.ModuleNumber);
        var now = _clock.Now;

        foreach (var module in modules)
        {
            module.ModuleNumber = reorderMap[module.ModuleID];
            module.UpdateAt = now;
            await _unitOfWork.Modules.UpdateAsync(module);
        }

        await _unitOfWork.SaveChangesAsync();

        var ordered = modules.OrderBy(m => m.ModuleNumber).ToList();
        return _mapper.Map<IEnumerable<ModuleClientViewDTO>>(ordered);
    }

    private async Task EnsureCourseVersionExistsAsync(Guid courseId, Guid versionId)
    {
        var courseVersion = await _unitOfWork.CourseVersions.GetByConditionAsync(
            v => v.CourseVersionID == versionId && v.CourseID == courseId);

        if (courseVersion == null)
            throw new BaseException("Course version not found.", "NOT_FOUND");
    }

    private async Task<Module> GetModuleEntityAsync(Guid courseId, Guid versionId, Guid moduleId)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var module = await _unitOfWork.Modules.GetByConditionAsync(
            m => m.ModuleID == moduleId && m.CourseVersionID == versionId);

        if (module == null)
            throw new BaseException("Module not found.", "NOT_FOUND");

        return module;
    }

    private async Task ValidateModuleNumberAsync(Guid versionId, int moduleNumber, Guid? excludeModuleId = null)
    {
        if (moduleNumber <= 0)
            throw new ValidationException("moduleNumber must be greater than 0.");

        var duplicated = await _unitOfWork.Modules.GetByConditionAsync(
            m => m.CourseVersionID == versionId
                 && m.ModuleNumber == moduleNumber
                 && (!excludeModuleId.HasValue || m.ModuleID != excludeModuleId.Value));

        if (duplicated != null)
            throw new ValidationException("moduleNumber must be unique in the course version.");
    }
}
