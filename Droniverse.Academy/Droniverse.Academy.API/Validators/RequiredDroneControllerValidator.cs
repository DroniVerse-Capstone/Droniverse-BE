using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.API.Validators;

public static class RequiredDroneControllerValidator
{
    public static void ValidateAddRequiredDrone(Guid courseId, Guid versionId, AddRequiredDroneRequestDTO request)
    {
        ValidateCourseVersionRoute(courseId, versionId);

        if (request == null)
            throw new ValidationException("Request body is required.");

        if (request.DroneID == Guid.Empty)
            throw new ValidationException("DroneID is required.");
    }

    public static void ValidateRemoveRequiredDrone(Guid courseId, Guid versionId, Guid droneId)
    {
        ValidateCourseVersionRoute(courseId, versionId);

        if (droneId == Guid.Empty)
            throw new ValidationException("DroneID is required.");
    }

    public static void ValidateGetRequiredDrones(Guid courseId, Guid versionId)
    {
        ValidateCourseVersionRoute(courseId, versionId);
    }

    public static void ValidateGetCourseVersionsByDrone(Guid droneId)
    {
        if (droneId == Guid.Empty)
            throw new ValidationException("DroneID is required.");
    }

    private static void ValidateCourseVersionRoute(Guid courseId, Guid versionId)
    {
        if (courseId == Guid.Empty)
            throw new ValidationException("CourseID is required.");

        if (versionId == Guid.Empty)
            throw new ValidationException("VersionID is required.");
    }
}
