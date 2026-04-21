using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services.Duplication;

public class WebSimulatorDuplicator : IWebSimulatorDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WebSimulatorDuplicator(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> DuplicateAsync(Guid sourceWebSimulatorId, CourseVersionDuplicationContext context)
    {
        if (context.WebSimulatorIdMap.TryGetValue(sourceWebSimulatorId, out var duplicatedWebSimulatorId))
            return duplicatedWebSimulatorId;

        var sourceWebSimulator = await _unitOfWork.WebSimulators.GetByIdAsync(sourceWebSimulatorId)
            ?? throw new BaseException("Không tìm thấy web simulator tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedWebSimulator = _mapper.Map<WebSimulator>(sourceWebSimulator);
        duplicatedWebSimulator.WebSimulatorID = Guid.NewGuid();
        duplicatedWebSimulator.SetAuditOnCreate(context.CurrentUserId, context.Now);

        await _unitOfWork.WebSimulators.AddAsync(duplicatedWebSimulator);

        context.WebSimulatorIdMap[sourceWebSimulatorId] = duplicatedWebSimulator.WebSimulatorID;
        return duplicatedWebSimulator.WebSimulatorID;
    }
}
