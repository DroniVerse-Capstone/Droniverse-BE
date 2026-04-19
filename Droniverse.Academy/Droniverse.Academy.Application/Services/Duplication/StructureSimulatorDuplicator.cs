using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services.Duplication;

public class StructureSimulatorDuplicator : IStructureSimulatorDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StructureSimulatorDuplicator(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> DuplicateAsync(Guid sourceStructureId, CourseVersionDuplicationContext context)
    {
        if (context.StructureSimulatorIdMap.TryGetValue(sourceStructureId, out var duplicatedStructureId))
            return duplicatedStructureId;

        var sourceStructure = await _unitOfWork.StructureSimulators.GetByIdAsync(sourceStructureId)
            ?? throw new BaseException("Không tìm thấy structure simulator tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedStructure = _mapper.Map<StructureSimulator>(sourceStructure);
        duplicatedStructure.StructureID = Guid.NewGuid();
        duplicatedStructure.SetAuditOnCreate(context.CurrentUserId, context.Now);

        await _unitOfWork.StructureSimulators.AddAsync(duplicatedStructure);

        context.StructureSimulatorIdMap[sourceStructureId] = duplicatedStructure.StructureID;
        return duplicatedStructure.StructureID;
    }
}
