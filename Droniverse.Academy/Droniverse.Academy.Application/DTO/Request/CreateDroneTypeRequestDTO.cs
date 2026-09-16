namespace Droniverse.Academy.Application.DTO.Request;

public class CreateDroneTypeRequestDTO
{
    public string TypeNameVN { get; set; } = null!;
    public string TypeNameEN { get; set; } = null!;
    public string DescriptionVN { get; set; } = null!;
    public string DescriptionEN { get; set; } = null!;
}
