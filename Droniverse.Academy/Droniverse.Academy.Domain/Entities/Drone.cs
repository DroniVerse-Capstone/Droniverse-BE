using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Drone
{
    public Guid DroneID { get; set; } //char(36)
    public DroneType DroneType { get; set; }
    public Guid DroneTypeID { get; set; } //char(36)
    public string DroneNameVN { get; set; } //nvarchar(255)
    public string DroneNameEN { get; set; } //nvarchar(255)
    public string Manufacturer { get; set; } //nvarchar(100)
    public string DescriptionVN { get; set; } //text
    public string DescriptionEN { get; set; } //text
    public float Height { get; set; } //float
    public float Weight { get; set; } //float
    public DroneStatus Status { get; set; }
    public string Model3DLink { get; set; } //text
    public ICollection<RequiredDrone> RequiredDrones { get; set; }

}
