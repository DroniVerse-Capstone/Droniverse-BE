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

    public async Task<DroneClientViewDTO> AddRequiredDroneAsync(Guid courseId, Guid versionId, AddRequiredDroneRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.DroneID == Guid.Empty)
            throw new ValidationException("DroneID là bắt buộc.");

        await EnsureCourseVersionExistsAsync(courseId, versionId);

        var drone = await _unitOfWork.Drones.GetByConditionAsync(
            d => d.DroneID == request.DroneID,
            includeProperties: "DroneType");

        if (drone == null)
            throw new BaseException("Không tìm thấy drone.", "NOT_FOUND");

        var exists = await _unitOfWork.RequiredDrones.GetByConditionAsync(
            rd => rd.CourseVersionID == versionId && rd.DroneID == request.DroneID);

        if (exists != null)
            throw new ValidationException("Drone này đã được yêu cầu cho phiên bản khóa học này.");

        var requiredDrone = new RequiredDrone
        {
            CourseVersionID = versionId,
            DroneID = request.DroneID
        };

        await _unitOfWork.RequiredDrones.AddAsync(requiredDrone);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DroneClientViewDTO>(drone);
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
