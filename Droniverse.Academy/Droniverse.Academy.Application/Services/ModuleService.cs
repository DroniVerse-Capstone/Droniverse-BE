using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

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

        var module = _mapper.Map<Module>(request);
        module.ModuleID = Guid.NewGuid();
        module.CourseVersionID = versionId;
        module.CreateAt = _clock.Now;
        module.UpdateAt = _clock.Now;

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

        _mapper.Map(request, module);
        module.UpdateAt = _clock.Now;

        await _unitOfWork.Modules.UpdateAsync(module);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ModuleClientViewDTO>(module);
    }

    public async Task DeleteModuleAsync(Guid courseId, Guid versionId, Guid moduleId)
    {
        var module = await GetModuleEntityAsync(courseId, versionId, moduleId);
        var deletedModuleNumber = module.ModuleNumber;

        await _unitOfWork.Modules.DeleteAsync(module);

        var modulesAfterDeleted = await _unitOfWork.Modules.GetAllAsync(
            filter: m => m.CourseVersionID == versionId && m.ModuleNumber > deletedModuleNumber,
            orderBy: q => q.OrderBy(m => m.ModuleNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var now = _clock.Now;
        foreach (var item in modulesAfterDeleted.Data)
        {
            item.ModuleNumber--;
            item.UpdateAt = now;
            await _unitOfWork.Modules.UpdateAsync(item);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ModuleClientViewDTO>> ReorderModulesAsync(Guid courseId, Guid versionId, ReorderModulesRequestDTO request)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        if (request.Modules.Count == 0)
            throw new ValidationException("Dữ liệu sắp xếp lại mô-đun là bắt buộc.");

        if (request.Modules.Select(x => x.ModuleID).Distinct().Count() != request.Modules.Count)
            throw new ValidationException("Dữ liệu sắp xếp lại chứa moduleId bị trùng.");

        if (request.Modules.Select(x => x.ModuleNumber).Distinct().Count() != request.Modules.Count)
            throw new ValidationException("moduleNumber phải là duy nhất trong dữ liệu sắp xếp lại.");

        if (request.Modules.Any(x => x.ModuleNumber <= 0))
            throw new ValidationException("moduleNumber phải lớn hơn 0.");

        var modulesResult = await _unitOfWork.Modules.GetAllAsync(
            filter: m => m.CourseVersionID == versionId,
            orderBy: q => q.OrderBy(m => m.ModuleNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var modules = modulesResult.Data.ToList();
        if (modules.Count != request.Modules.Count)
            throw new ValidationException("Dữ liệu sắp xếp lại phải chứa đầy đủ tất cả mô-đun của phiên bản khóa học.");

        var moduleIds = modules.Select(m => m.ModuleID).OrderBy(x => x).ToList();
        var requestIds = request.Modules.Select(m => m.ModuleID).OrderBy(x => x).ToList();
        if (!moduleIds.SequenceEqual(requestIds))
            throw new ValidationException("Dữ liệu sắp xếp lại chứa moduleId không hợp lệ.");

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
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");
    }

    private async Task<Module> GetModuleEntityAsync(Guid courseId, Guid versionId, Guid moduleId)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var module = await _unitOfWork.Modules.GetByConditionAsync(
            m => m.ModuleID == moduleId && m.CourseVersionID == versionId);

        if (module == null)
            throw new BaseException("Không tìm thấy mô-đun.", "NOT_FOUND");

        return module;
    }

    private async Task ValidateModuleNumberAsync(Guid versionId, int moduleNumber, Guid? excludeModuleId = null)
    {
        if (moduleNumber <= 0)
            throw new ValidationException("moduleNumber phải lớn hơn 0.");

        var duplicated = await _unitOfWork.Modules.GetByConditionAsync(
            m => m.CourseVersionID == versionId
                 && m.ModuleNumber == moduleNumber
                 && (!excludeModuleId.HasValue || m.ModuleID != excludeModuleId.Value));

        if (duplicated != null)
            throw new ValidationException("moduleNumber phải là duy nhất trong phiên bản khóa học.");
    }
}
