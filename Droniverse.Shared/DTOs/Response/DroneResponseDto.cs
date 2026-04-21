namespace Droniverse.Shared.DTOs.Response;

public record DroneResponseDto
{
    public Guid DroneID { get; set; }
    public Guid DroneTypeID { get; set; }
    public string DroneTypeNameVN { get; set; } = null!;
    public string DroneTypeNameEN { get; set; } = null!;
    public string DroneNameVN { get; set; } = null!;
    public string DroneNameEN { get; set; } = null!;
    public string Manufacturer { get; set; } = null!;
    public string DescriptionVN { get; set; } = null!;
    public string DescriptionEN { get; set; } = null!;
    public float Height { get; set; }
    public float Weight { get; set; }
    public DroneStatus Status { get; set; }
    public string ImgURL { get; set; } = null!;
}

public enum DroneStatus
{
    ACTIVE = 1,
    INACTIVE = 2,
    DEPRECATED = 3
}