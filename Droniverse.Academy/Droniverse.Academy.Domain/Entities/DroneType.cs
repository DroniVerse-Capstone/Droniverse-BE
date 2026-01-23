namespace Droniverse.Academy.Domain.Entities;
public class DroneType
{
    public Guid DroneTypeID { get; set; } //char(36)
    public string TypeNameVN { get; set; } //nvarchar(255)
    public string TypeNameEN { get; set; } //nvarchar(255)
    public string DescriptionVN { get; set; } //text
    public string DescriptionEN { get; set; } //text
    public ICollection<Drone> Drones { get; set; }

}
