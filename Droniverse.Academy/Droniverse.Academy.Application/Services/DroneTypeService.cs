using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class DroneTypeService : IDroneTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DroneTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DroneTypeClientViewDTO> CreateDroneTypeAsync(CreateDroneTypeRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateDroneTypeData(request.TypeNameVN, request.TypeNameEN);

        var droneType = _mapper.Map<DroneType>(request);
        droneType.DroneTypeID = Guid.NewGuid();

        await _unitOfWork.DroneTypes.AddAsync(droneType);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DroneTypeClientViewDTO>(droneType);
    }

    public async Task<IEnumerable<DroneTypeClientViewDTO>> GetDroneTypesAsync()
    {
        var droneTypes = await _unitOfWork.DroneTypes.GetAllAsync(
            orderBy: q => q.OrderBy(x => x.TypeNameEN),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return _mapper.Map<IEnumerable<DroneTypeClientViewDTO>>(droneTypes.Data);
    }

    public async Task<DroneTypeClientViewDTO> GetDroneTypeByIdAsync(Guid droneTypeId)
    {
        var droneType = await _unitOfWork.DroneTypes.GetByIdAsync(droneTypeId);
        if (droneType == null)
            throw new BaseException("Không tìm thấy loại drone.", "NOT_FOUND");

        return _mapper.Map<DroneTypeClientViewDTO>(droneType);
    }

    public async Task<DroneTypeClientViewDTO> UpdateDroneTypeAsync(Guid droneTypeId, UpdateDroneTypeRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateDroneTypeData(request.TypeNameVN, request.TypeNameEN);

        var droneType = await _unitOfWork.DroneTypes.GetByIdAsync(droneTypeId);
        if (droneType == null)
            throw new BaseException("Không tìm thấy loại drone.", "NOT_FOUND");

        _mapper.Map(request, droneType);

        await _unitOfWork.DroneTypes.UpdateAsync(droneType);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DroneTypeClientViewDTO>(droneType);
    }

    public async Task DeleteDroneTypeAsync(Guid droneTypeId)
    {
        var droneType = await _unitOfWork.DroneTypes.GetByIdAsync(droneTypeId);
        if (droneType == null)
            throw new BaseException("Không tìm thấy loại drone.", "NOT_FOUND");

        var inUse = await _unitOfWork.Drones.GetByConditionAsync(d => d.DroneTypeID == droneTypeId);
        if (inUse != null)
            throw new ValidationException("Không thể xóa loại drone vì đang có drone sử dụng.");

        await _unitOfWork.DroneTypes.DeleteAsync(droneType);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<DroneClientViewDTO> CreateDroneAsync(Guid droneTypeId, CreateDroneRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        await EnsureDroneTypeExistsAsync(droneTypeId);
        ValidateDroneData(request.DroneNameVN, request.DroneNameEN, request.Height, request.Weight);

        var drone = _mapper.Map<Drone>(request);
        drone.DroneID = Guid.NewGuid();
        drone.DroneTypeID = droneTypeId;

        await _unitOfWork.Drones.AddAsync(drone);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Drones.GetByConditionAsync(
            d => d.DroneID == drone.DroneID,
            includeProperties: "DroneType");

        return _mapper.Map<DroneClientViewDTO>(created ?? drone);
    }

    public async Task<IEnumerable<DroneClientViewDTO>> GetDronesByTypeAsync(Guid droneTypeId)
    {
        await EnsureDroneTypeExistsAsync(droneTypeId);

        var drones = await _unitOfWork.Drones.GetAllAsync(
            filter: d => d.DroneTypeID == droneTypeId,
            orderBy: q => q.OrderBy(x => x.DroneNameEN),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "DroneType");

        return _mapper.Map<IEnumerable<DroneClientViewDTO>>(drones.Data);
    }

    private async Task EnsureDroneTypeExistsAsync(Guid droneTypeId)
    {
        var droneType = await _unitOfWork.DroneTypes.GetByIdAsync(droneTypeId);
        if (droneType == null)
            throw new BaseException("Không tìm thấy loại drone.", "NOT_FOUND");
    }

    private static void ValidateDroneTypeData(string typeNameVN, string typeNameEN)
    {
        if (string.IsNullOrWhiteSpace(typeNameVN))
            throw new ValidationException("Tên loại drone tiếng Việt là bắt buộc.");

        if (string.IsNullOrWhiteSpace(typeNameEN))
            throw new ValidationException("Tên loại drone tiếng Anh là bắt buộc.");
    }

    private static void ValidateDroneData(string droneNameVN, string droneNameEN, float height, float weight)
    {
        if (string.IsNullOrWhiteSpace(droneNameVN))
            throw new ValidationException("Tên drone tiếng Việt là bắt buộc.");

        if (string.IsNullOrWhiteSpace(droneNameEN))
            throw new ValidationException("Tên drone tiếng Anh là bắt buộc.");

        if (height <= 0)
            throw new ValidationException("Chiều cao phải lớn hơn 0.");

        if (weight <= 0)
            throw new ValidationException("Khối lượng phải lớn hơn 0.");
    }
}
