using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services.Duplication;

public class LabDuplicator : ILabDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LabDuplicator(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> DuplicateAsync(Guid sourceLabId, CourseVersionDuplicationContext context)
    {
        if (context.LabIdMap.TryGetValue(sourceLabId, out var duplicatedLabId))
            return duplicatedLabId;

        var sourceLab = await _unitOfWork.Labs.GetByIdAsync(sourceLabId)
            ?? throw new BaseException("Không tìm thấy bài lab tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedLab = _mapper.Map<Lab>(sourceLab);
        duplicatedLab.LabID = Guid.NewGuid();
        duplicatedLab.SetAuditOnCreate(context.CurrentUserId, context.Now);

        await _unitOfWork.Labs.AddAsync(duplicatedLab);

        context.LabIdMap[sourceLabId] = duplicatedLab.LabID;
        context.LabContentSyncQueue.Add((sourceLabId, duplicatedLab.LabID));

        return duplicatedLab.LabID;
    }
}
