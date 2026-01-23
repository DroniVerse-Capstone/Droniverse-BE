namespace Droniverse.Academy.Domain.Entities;
public class RequiredDrone
{
    public Drone Drone { get; set; }
    public Guid DroneID { get; set; }
    public CourseVersion CourseVersion { get; set; }
    public Guid CourseVersionID { get; set; }
}
