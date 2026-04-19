using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services.Duplication;

public class FlightSimulatorDuplicator : IFlightSimulatorDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FlightSimulatorDuplicator(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> DuplicateAsync(Guid sourceFlightId, CourseVersionDuplicationContext context)
    {
        if (context.FlightSimulatorIdMap.TryGetValue(sourceFlightId, out var duplicatedFlightId))
            return duplicatedFlightId;

        var sourceFlight = await _unitOfWork.FlightSimulators.GetByIdAsync(sourceFlightId)
            ?? throw new BaseException("Không tìm thấy flight simulator tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedFlight = _mapper.Map<FlightSimulator>(sourceFlight);
        duplicatedFlight.FlightID = Guid.NewGuid();
        duplicatedFlight.SetAuditOnCreate(context.CurrentUserId, context.Now);

        await _unitOfWork.FlightSimulators.AddAsync(duplicatedFlight);

        context.FlightSimulatorIdMap[sourceFlightId] = duplicatedFlight.FlightID;
        return duplicatedFlight.FlightID;
    }
}
