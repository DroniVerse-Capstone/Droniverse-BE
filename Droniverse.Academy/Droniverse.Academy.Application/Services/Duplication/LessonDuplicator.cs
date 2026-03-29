using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;

namespace Droniverse.Academy.Application.Services.Duplication;

public class LessonDuplicator : ILessonDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILessonReferenceDuplicator _lessonReferenceDuplicator;

    public LessonDuplicator(IUnitOfWork unitOfWork, IMapper mapper, ILessonReferenceDuplicator lessonReferenceDuplicator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _lessonReferenceDuplicator = lessonReferenceDuplicator;
    }

    public async Task DuplicateAsync(CourseVersionDuplicationContext context)
    {
        if (context.ModuleIdMap.Count == 0)
            return;
        // Lấy danh sách Lesson của tất cả Module đã sao chép, sắp xếp theo OrderIndex để đảm bảo thứ tự khi sao chép.
        var sourceLessons = await GetSourceLessonsAsync(context.ModuleIdMap.Keys.ToList());
        foreach (var sourceLesson in sourceLessons)
        {
            // Sao chép Lesson, gán ModuleID mới từ context.ModuleIdMap và tạo ReferenceID mới bằng cách gọi _lessonReferenceDuplicator.
            var duplicatedLesson = _mapper.Map<Lesson>(sourceLesson);
            duplicatedLesson.LessonID = Guid.NewGuid();
            // Đây là id của module mới đã được sao chép 
            duplicatedLesson.ModuleID = context.ModuleIdMap[sourceLesson.ModuleID];
            // Tạo ReferenceID mới cho bài học đã sao chép bằng cách gọi _lessonReferenceDuplicator. Tham số truyền vào là bài học gốc và context để lấy thông tin cần thiết cho việc sao chép.
            duplicatedLesson.ReferenceID = await _lessonReferenceDuplicator.DuplicateAsync(sourceLesson, context);

            await _unitOfWork.Lessons.AddAsync(duplicatedLesson);
        }
    }

    private async Task<IReadOnlyCollection<Lesson>> GetSourceLessonsAsync(IReadOnlyCollection<Guid> sourceModuleIds)
    {
        var sourceLessonsResult = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => sourceModuleIds.Contains(l.ModuleID),
            orderBy: q => q.OrderBy(l => l.OrderIndex),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return sourceLessonsResult.Data
            .OrderBy(x => x.OrderIndex)
            .ToList();
    }
}
