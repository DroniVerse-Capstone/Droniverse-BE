using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class UpdateDroneRequestDTO
{
    public Guid DroneTypeID { get; set; }
    public string DroneNameVN { get; set; } = null!;
    public string DroneNameEN { get; set; } = null!;
    public string Manufacturer { get; set; } = null!;
    public string DescriptionVN { get; set; } = null!;
    public string DescriptionEN { get; set; } = null!;
    public float Height { get; set; }
    public float Weight { get; set; }
    public DroneStatus Status { get; set; }
    public string Model3DLink { get; set; } = null!;
}
