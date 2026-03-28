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

        var sourceLessons = await GetSourceLessonsAsync(context.ModuleIdMap.Keys.ToList());
        foreach (var sourceLesson in sourceLessons)
        {
            var duplicatedLesson = _mapper.Map<Lesson>(sourceLesson);
            duplicatedLesson.LessonID = Guid.NewGuid();
            duplicatedLesson.ModuleID = context.ModuleIdMap[sourceLesson.ModuleID];
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
