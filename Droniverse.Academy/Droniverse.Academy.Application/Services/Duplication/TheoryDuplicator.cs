using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services.Duplication;

public class TheoryDuplicator : ITheoryDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TheoryDuplicator(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> DuplicateAsync(Guid sourceTheoryId, CourseVersionDuplicationContext context)
    {
        if (context.TheoryIdMap.TryGetValue(sourceTheoryId, out var duplicatedTheoryId))
            return duplicatedTheoryId;

        var sourceTheory = await _unitOfWork.Theories.GetByIdAsync(sourceTheoryId)
            ?? throw new BaseException("Không tìm thấy bài lý thuyết tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedTheory = _mapper.Map<Theory>(sourceTheory);
        duplicatedTheory.TheoryID = Guid.NewGuid();
        duplicatedTheory.SetAuditOnCreate(context.CurrentUserId, context.Now);

        await _unitOfWork.Theories.AddAsync(duplicatedTheory);

        context.TheoryIdMap[sourceTheoryId] = duplicatedTheory.TheoryID;
        return duplicatedTheory.TheoryID;
    }
}
