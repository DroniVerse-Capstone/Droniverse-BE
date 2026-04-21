using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services.Duplication;

public class VRSimulatorDuplicator : IVRSimulatorDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VRSimulatorDuplicator(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> DuplicateAsync(Guid sourceVRSimulatorId, CourseVersionDuplicationContext context)
    {
        if (context.VRSimulatorIdMap.TryGetValue(sourceVRSimulatorId, out var duplicatedVRSimulatorId))
            return duplicatedVRSimulatorId;

        var sourceVRSimulator = await _unitOfWork.VRSimulators.GetByIdAsync(sourceVRSimulatorId)
            ?? throw new BaseException("Không tìm thấy vr simulator tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedVRSimulator = _mapper.Map<VRSimulator>(sourceVRSimulator);
        duplicatedVRSimulator.VRSimulatorID = Guid.NewGuid();
        duplicatedVRSimulator.SetAuditOnCreate(context.CurrentUserId, context.Now);

        await _unitOfWork.VRSimulators.AddAsync(duplicatedVRSimulator);

        context.VRSimulatorIdMap[sourceVRSimulatorId] = duplicatedVRSimulator.VRSimulatorID;
        return duplicatedVRSimulator.VRSimulatorID;
    }
}
