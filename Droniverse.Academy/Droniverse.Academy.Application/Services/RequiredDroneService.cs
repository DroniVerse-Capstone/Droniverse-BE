using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class RequiredDroneService : IRequiredDroneService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RequiredDroneService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DroneClientViewDTO>> AddRequiredDronesAsync(Guid courseId, Guid versionId, AddRequiredDronesRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.DroneIDs.Count == 0)
            throw new ValidationException("Danh sách DroneID là bắt buộc.");

        var droneIds = request.DroneIDs.Distinct().ToList();
        if (droneIds.Any(id => id == Guid.Empty))
            throw new ValidationException("DroneID không hợp lệ.");

        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var dronesResult = await _unitOfWork.Drones.GetAllAsync(
            filter: d => droneIds.Contains(d.DroneID),
            orderBy: q => q.OrderBy(x => x.DroneNameEN),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "DroneType");

        var drones = dronesResult.Data.ToList();
        if (drones.Count != droneIds.Count)
            throw new BaseException("Có drone không tồn tại.", "NOT_FOUND");

        var existsResult = await _unitOfWork.RequiredDrones.GetAllAsync(
            filter: rd => rd.CourseVersionID == versionId && droneIds.Contains(rd.DroneID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        if (existsResult.Data.Any())
            throw new ValidationException("Có drone đã được yêu cầu cho phiên bản khóa học này.");

        foreach (var droneId in droneIds)
        {
            await _unitOfWork.RequiredDrones.AddAsync(new RequiredDrone
            {
                CourseVersionID = versionId,
                DroneID = droneId
            });
        }

        await _unitOfWork.SaveChangesAsync();

        var droneMap = drones.ToDictionary(d => d.DroneID, d => d);
        var ordered = droneIds.Select(id => droneMap[id]).ToList();
        return _mapper.Map<IEnumerable<DroneClientViewDTO>>(ordered);
    }

    public async Task RemoveRequiredDroneAsync(Guid courseId, Guid versionId, Guid droneId)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var requiredDrone = await _unitOfWork.RequiredDrones.GetByConditionAsync(
            rd => rd.CourseVersionID == versionId && rd.DroneID == droneId);

        if (requiredDrone == null)
            throw new BaseException("Không tìm thấy drone yêu cầu cho phiên bản khóa học này.", "NOT_FOUND");

        await _unitOfWork.RequiredDrones.DeleteAsync(requiredDrone);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<DroneClientViewDTO>> GetRequiredDronesAsync(Guid courseId, Guid versionId)
    {
        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var requiredDrones = await _unitOfWork.RequiredDrones.GetAllAsync(
            filter: rd => rd.CourseVersionID == versionId,
            orderBy: q => q.OrderBy(rd => rd.Drone.DroneNameEN),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "Drone,Drone.DroneType");

        var drones = requiredDrones.Data.Select(x => x.Drone).Where(x => x != null);
        return _mapper.Map<IEnumerable<DroneClientViewDTO>>(drones);
    }

    public async Task<IEnumerable<CourseVersionByDroneClientViewDTO>> GetCourseVersionsByDroneAsync(Guid droneId)
    {
        var drone = await _unitOfWork.Drones.GetByIdAsync(droneId);
        if (drone == null)
            throw new BaseException("Không tìm thấy drone.", "NOT_FOUND");

        var requiredDrones = await _unitOfWork.RequiredDrones.GetAllAsync(
            filter: rd => rd.DroneID == droneId,
            orderBy: q => q.OrderByDescending(rd => rd.CourseVersion.Version),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "CourseVersion");

        var courseVersions = requiredDrones.Data
            .Select(x => x.CourseVersion)
            .Where(x => x != null)
            .DistinctBy(x => x.CourseVersionID)
            .ToList();

        return _mapper.Map<IEnumerable<CourseVersionByDroneClientViewDTO>>(courseVersions);
    }

    private async Task EnsureCourseVersionExistsAsync(Guid courseId, Guid versionId)
    {
        var courseVersion = await _unitOfWork.CourseVersions.GetByConditionAsync(
            cv => cv.CourseVersionID == versionId && cv.CourseID == courseId);

        if (courseVersion == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");
    }
}
