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
        // Tạo bảng sao Version
        var duplicatedVersion = BuildDuplicatedVersion(course, sourceVersion, nextVersion, currentUserId, now);
        await _unitOfWork.CourseVersions.AddAsync(duplicatedVersion);

        var context = new CourseVersionDuplicationContext
        {
            SourceVersion = sourceVersion,
            DuplicatedVersion = duplicatedVersion,
            CurrentUserId = currentUserId,
            Now = now
        };
        // Sao chép Module, Theory, Quiz, Lab
        await _moduleDuplicator.DuplicateAsync(context);
        // Trả về kết quả bao gồm Version đã sao chép và danh sách Lab cần đồng bộ nội dung
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
}
