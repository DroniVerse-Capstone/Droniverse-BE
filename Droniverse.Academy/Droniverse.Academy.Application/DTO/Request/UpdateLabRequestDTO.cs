using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class UpdateLabRequestDTO
{
    public LabType Type { get; set; }
    public LabLevel Level { get; set; }
    public LabStatus Status { get; set; }
    public int EstimatedTime { get; set; }
    public string NameVN { get; set; } = null!;
    public string NameEN { get; set; } = null!;
    public string DescriptionVN { get; set; } = null!;
    public string DescriptionEN { get; set; } = null!;
}
