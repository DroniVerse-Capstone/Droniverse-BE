using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class CreateLabRequestDTO
{
    public LabType Type { get; set; }
    public LabLevel Level { get; set; } = LabLevel.EASY;
    public LabStatus Status { get; set; } = LabStatus.DRAFT;
    public int EstimatedTime { get; set; }
    public string NameVN { get; set; } = null!;
    public string NameEN { get; set; } = null!;
    public string DescriptionVN { get; set; } = null!;
    public string DescriptionEN { get; set; } = null!;
}
