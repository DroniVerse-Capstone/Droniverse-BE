using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.IService.Mongo;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;

public class CourseVersionService : ICourseVersionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;
    private readonly IUserDisplayNameService _userDisplayNameService;
    private readonly ILabContentService _labContentService;

    public CourseVersionService(
        IUnitOfWork unitOfWork,
        ICurrentUserService current,
        IClock clock,
        IMapper mapper,
        IUserDisplayNameService userDisplayNameService,
        ILabContentService labContentService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = current;
        _clock = clock;
        _mapper = mapper;
        _userDisplayNameService = userDisplayNameService;
        _labContentService = labContentService;
    }

    public async Task<CourseVersionResponseDTO> CreateCourseVersionAsync(Guid courseId, CreateCourseVersionRequestDTO request)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

        // determine next version number
        var nextVersion = 1;
        if (course.CourseVersions != null && course.CourseVersions.Any())
        {
            nextVersion = course.CourseVersions.Max(v => v.Version) + 1;
        }

        var cv = _mapper.Map<CourseVersion>(request);
        cv.CourseVersionID = Guid.NewGuid();
        cv.CourseID = course.CourseID;
        cv.Version = nextVersion;
        cv.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        if (nextVersion == 1)
        {
            course.CurrentVersion = cv;
            course.CurrentVersionID = cv.CourseVersionID;
        }

        await _unitOfWork.CourseVersions.AddAsync(cv);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CourseVersionResponseDTO>(cv);
        await PopulateUpdaterAsync(response, cv.UpdateBy);

        return response;
    }

    public async Task DeleteCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (course.CurrentVersionID == versionId)
            throw new ValidationException("Không thể xóa phiên bản hiện tại.");

        if (cv.Status == CourseVersionStatus.INACTIVE)
            throw new ValidationException("Phiên bản đã ở trạng thái đã xóa mềm, chỉ được xem.");

        if (cv.Status != CourseVersionStatus.DRAFT && cv.Status != CourseVersionStatus.DEPRECATED)
            throw new ValidationException("Chỉ phiên bản ở trạng thái Draft hoặc Deprecated mới có thể xóa mềm.");

        cv.Inactivate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.CourseVersions.UpdateAsync(cv);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CourseVersionResponseDTO> GetCourseVersionByIdAsync(Guid courseId, Guid versionId)
    {
        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId, includeProperties: "CourseVersionCategories,RequiredDrones");
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        var response = _mapper.Map<CourseVersionResponseDTO>(cv);
        await PopulateUpdaterAsync(response, cv.UpdateBy);

        return response;
    }

    public async Task<CourseVersionResponseDTO> DuplicateCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

        var sourceVersion = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == versionId);
        if (sourceVersion == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (sourceVersion.Status == CourseVersionStatus.INACTIVE)
            throw new ValidationException("Không thể nhân bản phiên bản đã xóa mềm.");

        var nextVersion = course.CourseVersions.Any()
            ? course.CourseVersions.Max(v => v.Version) + 1
            : 1;

        var now = _clock.Now;

        var duplicatedVersion = _mapper.Map<CourseVersion>(sourceVersion);
        duplicatedVersion.CourseVersionID = Guid.NewGuid();
        duplicatedVersion.CourseID = course.CourseID;
        duplicatedVersion.Version = nextVersion;
        duplicatedVersion.SetAuditOnCreate(_currentUser.UserId, now);

        await _unitOfWork.CourseVersions.AddAsync(duplicatedVersion);

        var sourceCategories = await _unitOfWork.CourseVersionCategories.GetAllAsync(
            filter: x => x.CourseVersionID == sourceVersion.CourseVersionID,
            pageIndex: 1,
            pageSize: int.MaxValue);

        foreach (var categoryId in sourceCategories.Data.Select(x => x.CategoryID).Distinct())
        {
            await _unitOfWork.CourseVersionCategories.AddAsync(new CourseVersionCategory
            {
                CourseVersionID = duplicatedVersion.CourseVersionID,
                CategoryID = categoryId
            });
        }

        var sourceRequiredDrones = await _unitOfWork.RequiredDrones.GetAllAsync(
            filter: x => x.CourseVersionID == sourceVersion.CourseVersionID,
            pageIndex: 1,
            pageSize: int.MaxValue);

        foreach (var droneId in sourceRequiredDrones.Data.Select(x => x.DroneID).Distinct())
        {
            await _unitOfWork.RequiredDrones.AddAsync(new RequiredDrone
            {
                CourseVersionID = duplicatedVersion.CourseVersionID,
                DroneID = droneId
            });
        }

        var sourceModulesResult = await _unitOfWork.Modules.GetAllAsync(
            filter: m => m.CourseVersionID == sourceVersion.CourseVersionID,
            orderBy: q => q.OrderBy(m => m.ModuleNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var sourceModules = sourceModulesResult.Data.ToList();
        var moduleIdMap = new Dictionary<Guid, Guid>();
        var theoryIdMap = new Dictionary<Guid, Guid>();
        var quizIdMap = new Dictionary<Guid, Guid>();
        var labIdMap = new Dictionary<Guid, Guid>();
        var labContentSyncQueue = new List<(Guid SourceLabId, Guid NewLabId)>();

        foreach (var sourceModule in sourceModules)
        {
            var newModuleId = Guid.NewGuid();
            moduleIdMap[sourceModule.ModuleID] = newModuleId;

            var duplicatedModule = _mapper.Map<Module>(sourceModule);
            duplicatedModule.ModuleID = newModuleId;
            duplicatedModule.CourseVersionID = duplicatedVersion.CourseVersionID;
            duplicatedModule.CreateAt = now;
            duplicatedModule.UpdateAt = now;

            await _unitOfWork.Modules.AddAsync(duplicatedModule);
        }

        if (moduleIdMap.Count > 0)
        {
            var sourceModuleIds = moduleIdMap.Keys.ToList();
            var sourceLessonsResult = await _unitOfWork.Lessons.GetAllAsync(
                filter: l => sourceModuleIds.Contains(l.ModuleID),
                orderBy: q => q.OrderBy(l => l.OrderIndex),
                pageIndex: 1,
                pageSize: int.MaxValue);

            foreach (var sourceLesson in sourceLessonsResult.Data.OrderBy(l => l.OrderIndex))
            {
                var duplicatedLesson = _mapper.Map<Lesson>(sourceLesson);
                duplicatedLesson.LessonID = Guid.NewGuid();
                duplicatedLesson.ModuleID = moduleIdMap[sourceLesson.ModuleID];
                duplicatedLesson.ReferenceID = await DuplicateLessonReferenceAsync(
                    sourceLesson,
                    now,
                    theoryIdMap,
                    quizIdMap,
                    labIdMap,
                    labContentSyncQueue);

                await _unitOfWork.Lessons.AddAsync(duplicatedLesson);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        foreach (var (sourceLabId, newLabId) in labContentSyncQueue)
        {
            var sourceContent = await _labContentService.GetByLabIdAsync(sourceLabId);
            if (sourceContent == null)
            {
                await _labContentService.CreateEmptyAsync(newLabId);
                continue;
            }

            await _labContentService.CreateEmptyAsync(newLabId);
            await _labContentService.UpdateByLabIdAsync(newLabId, new UpdateLabContentRequestDTO
            {
                Environment = sourceContent.Environment
            });
        }

        var response = _mapper.Map<CourseVersionResponseDTO>(duplicatedVersion);
        await PopulateUpdaterAsync(response, duplicatedVersion.UpdateBy);

        return response;
    }

    private async Task<Guid> DuplicateLessonReferenceAsync(
        Lesson sourceLesson,
        DateTime now,
        IDictionary<Guid, Guid> theoryIdMap,
        IDictionary<Guid, Guid> quizIdMap,
        IDictionary<Guid, Guid> labIdMap,
        ICollection<(Guid SourceLabId, Guid NewLabId)> labContentSyncQueue)
    {
        if (sourceLesson.ReferenceID == Guid.Empty)
            return Guid.Empty;

        return sourceLesson.Type switch
        {
            LessonType.THEORY => await DuplicateTheoryAsync(sourceLesson.ReferenceID, now, theoryIdMap),
            LessonType.QUIZ => await DuplicateQuizAsync(sourceLesson.ReferenceID, now, quizIdMap),
            LessonType.LAB => await DuplicateLabAsync(sourceLesson.ReferenceID, now, labIdMap, labContentSyncQueue),
            _ => sourceLesson.ReferenceID
        };
    }

    private async Task<Guid> DuplicateTheoryAsync(Guid sourceTheoryId, DateTime now, IDictionary<Guid, Guid> theoryIdMap)
    {
        if (theoryIdMap.TryGetValue(sourceTheoryId, out var mappedTheoryId))
            return mappedTheoryId;

        var sourceTheory = await _unitOfWork.Theories.GetByIdAsync(sourceTheoryId)
            ?? throw new BaseException("Không tìm thấy bài lý thuyết tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedTheory = _mapper.Map<Theory>(sourceTheory);
        duplicatedTheory.TheoryID = Guid.NewGuid();
        duplicatedTheory.SetAuditOnCreate(_currentUser.UserId, now);

        await _unitOfWork.Theories.AddAsync(duplicatedTheory);

        theoryIdMap[sourceTheoryId] = duplicatedTheory.TheoryID;
        return duplicatedTheory.TheoryID;
    }

    private async Task<Guid> DuplicateQuizAsync(Guid sourceQuizId, DateTime now, IDictionary<Guid, Guid> quizIdMap)
    {
        if (quizIdMap.TryGetValue(sourceQuizId, out var mappedQuizId))
            return mappedQuizId;

        var sourceQuiz = await _unitOfWork.Quizs.GetByIdAsync(sourceQuizId)
            ?? throw new BaseException("Không tìm thấy bài quiz tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedQuiz = _mapper.Map<Quiz>(sourceQuiz);
        duplicatedQuiz.QuizID = Guid.NewGuid();
        duplicatedQuiz.SetAuditOnCreate(_currentUser.UserId, now);

        await _unitOfWork.Quizs.AddAsync(duplicatedQuiz);

        var sourceQuestions = await _unitOfWork.QuizQuestions.GetAllAsync(
            filter: q => q.QuizID == sourceQuizId,
            orderBy: q => q.OrderBy(x => x.QuestionID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        foreach (var sourceQuestion in sourceQuestions.Data)
        {
            var duplicatedQuestion = _mapper.Map<QuizQuestion>(sourceQuestion);
            duplicatedQuestion.QuestionID = Guid.NewGuid();
            duplicatedQuestion.QuizID = duplicatedQuiz.QuizID;

            await _unitOfWork.QuizQuestions.AddAsync(duplicatedQuestion);
        }

        quizIdMap[sourceQuizId] = duplicatedQuiz.QuizID;
        return duplicatedQuiz.QuizID;
    }

    private async Task<Guid> DuplicateLabAsync(
        Guid sourceLabId,
        DateTime now,
        IDictionary<Guid, Guid> labIdMap,
        ICollection<(Guid SourceLabId, Guid NewLabId)> labContentSyncQueue)
    {
        if (labIdMap.TryGetValue(sourceLabId, out var mappedLabId))
            return mappedLabId;

        var sourceLab = await _unitOfWork.Labs.GetByIdAsync(sourceLabId)
            ?? throw new BaseException("Không tìm thấy bài lab tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedLab = _mapper.Map<Lab>(sourceLab);
        duplicatedLab.LabID = Guid.NewGuid();
        duplicatedLab.SetAuditOnCreate(_currentUser.UserId, now);

        await _unitOfWork.Labs.AddAsync(duplicatedLab);

        labIdMap[sourceLabId] = duplicatedLab.LabID;
        labContentSyncQueue.Add((sourceLabId, duplicatedLab.LabID));

        return duplicatedLab.LabID;
    }

    public async Task<PaginationResult<IEnumerable<CourseVersionResponseDTO>>> GetCourseVersionsAsync(Guid courseId, int pageIndex, int pageSize, CourseVersionStatus? status = null)
    {
        Expression<Func<CourseVersion, bool>>? filter = v => v.CourseID == courseId;
        if (status.HasValue)
        {
            var s = status.Value;
            filter = v => v.CourseID == courseId && v.Status == s;
        }

        var result = await _unitOfWork.CourseVersions.GetAllAsync(filter, null, pageIndex, pageSize, includeProperties: "CourseVersionCategories,RequiredDrones");
        var entities = result.Data.ToList();
        var mapped = entities.Select(v => _mapper.Map<CourseVersionResponseDTO>(v)).ToList();

        var userLookup = await BuildUpdaterLookupAsync(entities);
        PopulateMappedVersionsUpdater(entities, mapped, userLookup);

        return new PaginationResult<IEnumerable<CourseVersionResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task ActivateCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

        var cv = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == versionId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (cv.Status == CourseVersionStatus.INACTIVE)
            throw new ValidationException("Phiên bản đã xóa mềm chỉ được xem, không thể thao tác.");

        // deprecate other active versions
        var active = course.CourseVersions
            .Where(v => v.Status == CourseVersionStatus.ACTIVE && v.CourseVersionID != versionId)
            .ToList();
        foreach (var a in active)
        {
            a.Deprecate(_currentUser.UserId, _clock.Now);
        }

        cv.Activate(_currentUser.UserId, _clock.Now);
        // set current version
        course.CurrentVersion = cv;
        course.CurrentVersionID = cv.CourseVersionID;


        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateCourseVersionAsync(Guid courseId, Guid versionId)
    {
        throw new ValidationException("Không hỗ trợ vô hiệu hóa trực tiếp phiên bản. Hãy kích hoạt phiên bản khác để phiên bản đang Active chuyển sang Deprecated.");
    }

    public async Task<CourseVersionResponseDTO> UpdateCourseVersionAsync(Guid courseId, Guid versionId, UpdateCourseVersionRequestDTO request)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

        var cv = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == versionId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (cv.Status == CourseVersionStatus.INACTIVE)
            throw new ValidationException("Phiên bản đã xóa mềm chỉ được xem, không thể thao tác.");

        cv.UpdateContent(request.TitleVN, request.TitleEN, request.DescriptionVN, request.DescriptionEN, request.ContextVN, request.ContextEN, request.ImageUrl, request.Level, request.EstimatedDuration, request.ChangeLog, _currentUser.UserId, _clock.Now);

        await _unitOfWork.CourseVersions.UpdateAsync(cv);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CourseVersionResponseDTO>(cv);
        await PopulateUpdaterAsync(response, cv.UpdateBy);

        return response;
    }

    private async Task PopulateUpdaterAsync(CourseVersionResponseDTO courseVersion, Guid? updateBy)
    {
        if (!updateBy.HasValue || updateBy.Value == Guid.Empty)
            return;

        courseVersion.Updater = await _userDisplayNameService.ResolveUserDisplayNameAsync(updateBy.Value);
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUpdaterLookupAsync(IEnumerable<CourseVersion> versions)
    {
        var userIds = versions
            .Select(v => v.UpdateBy)
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var lookup = await _userDisplayNameService.ResolveUsersDisplayNameAsync(userIds);
        return lookup.ToDictionary(x => x.Key, x => x.Value);
    }

    private static void PopulateMappedVersionsUpdater(
        IEnumerable<CourseVersion> entities,
        IEnumerable<CourseVersionResponseDTO> dtos,
        IReadOnlyDictionary<Guid, SimpleUserReponse?> userLookup)
    {
        foreach (var (entity, dto) in entities.Zip(dtos))
        {
            var updaterId = entity.UpdateBy;
            if (updaterId.HasValue && updaterId.Value != Guid.Empty
                && userLookup.TryGetValue(updaterId.Value, out var updater))
            {
                dto.Updater = updater;
            }
        }
    }
}
