using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;

namespace Droniverse.Academy.Application.Services.Duplication;

public class ModuleDuplicator : IModuleDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILessonDuplicator _lessonDuplicator;

    public ModuleDuplicator(IUnitOfWork unitOfWork, IMapper mapper, ILessonDuplicator lessonDuplicator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _lessonDuplicator = lessonDuplicator;
    }

    public async Task DuplicateAsync(CourseVersionDuplicationContext context)
    {
        // Lấy danh sách Module của phiên bản nguồn, sắp xếp theo ModuleNumber để đảm bảo thứ tự khi sao chép.
        var sourceModules = await GetSourceModulesAsync(context.SourceVersion.CourseVersionID);

        // Sao chép từng Module, tạo ID mới và lưu mapping từ ID cũ sang ID mới.
        foreach (var sourceModule in sourceModules)
        {
            var duplicatedModule = _mapper.Map<Module>(sourceModule);
            var newModuleId = Guid.NewGuid();

            duplicatedModule.ModuleID = newModuleId;
            duplicatedModule.CourseVersionID = context.DuplicatedVersion.CourseVersionID;
            duplicatedModule.CreateAt = context.Now;
            duplicatedModule.UpdateAt = context.Now;
            // Lưu mapping từ ID cũ sang ID mới để sử dụng khi sao chép các Lesson.
            context.ModuleIdMap[sourceModule.ModuleID] = newModuleId;
            await _unitOfWork.Modules.AddAsync(duplicatedModule);
        }
        // Sao chép các leeson cho từng module
        await _lessonDuplicator.DuplicateAsync(context);
    }

    /**
     * Lấy danh sách Module của phiên bản nguồn, sắp xếp theo ModuleNumber để đảm bảo thứ tự khi sao chép.
     */
    private async Task<IReadOnlyCollection<Module>> GetSourceModulesAsync(Guid sourceVersionId)
    {
        var sourceModulesResult = await _unitOfWork.Modules.GetAllAsync(
            filter: m => m.CourseVersionID == sourceVersionId,
            orderBy: q => q.OrderBy(m => m.ModuleNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return sourceModulesResult.Data.ToList();
    }
}
