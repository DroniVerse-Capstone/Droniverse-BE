using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class DroneService : IDroneService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DroneService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DroneClientViewDTO>> GetDronesAsync()
    {
        var drones = await _unitOfWork.Drones.GetAllAsync(
            orderBy: q => q.OrderBy(x => x.DroneNameEN),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "DroneType");

        return _mapper.Map<IEnumerable<DroneClientViewDTO>>(drones.Data);
    }

    public async Task<DroneClientViewDTO> GetDroneByIdAsync(Guid droneId)
    {
        var drone = await _unitOfWork.Drones.GetByConditionAsync(
            d => d.DroneID == droneId,
            includeProperties: "DroneType");

        if (drone == null)
            throw new BaseException("Drone not found.", "NOT_FOUND");

        return _mapper.Map<DroneClientViewDTO>(drone);
    }

    public async Task<DroneClientViewDTO> UpdateDroneAsync(Guid droneId, UpdateDroneRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateDroneData(request.DroneNameVN, request.DroneNameEN, request.Height, request.Weight);

        var drone = await _unitOfWork.Drones.GetByConditionAsync(
            d => d.DroneID == droneId,
            includeProperties: "DroneType");

        if (drone == null)
            throw new BaseException("Drone not found.", "NOT_FOUND");

        var droneType = await _unitOfWork.DroneTypes.GetByIdAsync(request.DroneTypeID);
        if (droneType == null)
            throw new BaseException("Drone type not found.", "NOT_FOUND");

        _mapper.Map(request, drone);

        await _unitOfWork.Drones.UpdateAsync(drone);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Drones.GetByConditionAsync(
            d => d.DroneID == droneId,
            includeProperties: "DroneType");

        return _mapper.Map<DroneClientViewDTO>(updated ?? drone);
    }

    public async Task DeleteDroneAsync(Guid droneId)
    {
        var drone = await _unitOfWork.Drones.GetByIdAsync(droneId);
        if (drone == null)
            throw new BaseException("Drone not found.", "NOT_FOUND");

        var inUse = await _unitOfWork.RequiredDrones.GetByConditionAsync(rd => rd.DroneID == droneId);
        if (inUse != null)
            throw new ValidationException("Cannot delete drone because it is being used by course versions.");

        await _unitOfWork.Drones.DeleteAsync(drone);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void ValidateDroneData(string droneNameVN, string droneNameEN, float height, float weight)
    {
        if (string.IsNullOrWhiteSpace(droneNameVN))
            throw new ValidationException("DroneNameVN is required.");

        if (string.IsNullOrWhiteSpace(droneNameEN))
            throw new ValidationException("DroneNameEN is required.");

        if (height <= 0)
            throw new ValidationException("Height must be greater than 0.");

        if (weight <= 0)
            throw new ValidationException("Weight must be greater than 0.");
    }
}
