using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;

namespace Droniverse.Academy.Application.Services.Duplication;

public class CourseVersionDuplicator : ICourseVersionDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IModuleDuplicator _moduleDuplicator;

    public CourseVersionDuplicator(IUnitOfWork unitOfWork, IMapper mapper, IModuleDuplicator moduleDuplicator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _moduleDuplicator = moduleDuplicator;
    }

    public async Task<CourseVersionDuplicationResult> DuplicateAsync(
        Course course,
        CourseVersion sourceVersion,
        int nextVersion,
        Guid currentUserId,
        DateTime now)
    {
        var duplicatedVersion = BuildDuplicatedVersion(course, sourceVersion, nextVersion, currentUserId, now);
        await _unitOfWork.CourseVersions.AddAsync(duplicatedVersion);

        await DuplicateCategoriesAsync(sourceVersion.CourseVersionID, duplicatedVersion.CourseVersionID);
        await DuplicateRequiredDronesAsync(sourceVersion.CourseVersionID, duplicatedVersion.CourseVersionID);

        var context = new CourseVersionDuplicationContext
        {
            SourceVersion = sourceVersion,
            DuplicatedVersion = duplicatedVersion,
            CurrentUserId = currentUserId,
            Now = now
        };

        await _moduleDuplicator.DuplicateAsync(context);

        return new CourseVersionDuplicationResult
        {
            DuplicatedVersion = duplicatedVersion,
            LabContentSyncQueue = context.LabContentSyncQueue
        };
    }

    private CourseVersion BuildDuplicatedVersion(
        Course course,
        CourseVersion sourceVersion,
        int nextVersion,
        Guid currentUserId,
        DateTime now)
    {
        var duplicatedVersion = _mapper.Map<CourseVersion>(sourceVersion);
        duplicatedVersion.CourseVersionID = Guid.NewGuid();
        duplicatedVersion.CourseID = course.CourseID;
        duplicatedVersion.Version = nextVersion;
        duplicatedVersion.SetAuditOnCreate(currentUserId, now);

        return duplicatedVersion;
    }

    private async Task DuplicateCategoriesAsync(Guid sourceVersionId, Guid duplicatedVersionId)
    {
        var sourceCategories = await _unitOfWork.CourseVersionCategories.GetAllAsync(
            filter: x => x.CourseVersionID == sourceVersionId,
            pageIndex: 1,
            pageSize: int.MaxValue);

        foreach (var categoryId in sourceCategories.Data.Select(x => x.CategoryID).Distinct())
        {
            await _unitOfWork.CourseVersionCategories.AddAsync(new CourseVersionCategory
            {
                CourseVersionID = duplicatedVersionId,
                CategoryID = categoryId
            });
        }
    }

    private async Task DuplicateRequiredDronesAsync(Guid sourceVersionId, Guid duplicatedVersionId)
    {
        var sourceRequiredDrones = await _unitOfWork.RequiredDrones.GetAllAsync(
            filter: x => x.CourseVersionID == sourceVersionId,
            pageIndex: 1,
            pageSize: int.MaxValue);

        foreach (var droneId in sourceRequiredDrones.Data.Select(x => x.DroneID).Distinct())
        {
            await _unitOfWork.RequiredDrones.AddAsync(new RequiredDrone
            {
                CourseVersionID = duplicatedVersionId,
                DroneID = droneId
            });
        }
    }
}
