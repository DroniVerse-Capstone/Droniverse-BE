using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class LabClientViewDTO
{
    public Guid LabID { get; set; }
    public LabType Type { get; set; }
    public LabLevel Level { get; set; }
    public LabStatus Status { get; set; }
    public string NameVN { get; set; } = null!;
    public string NameEN { get; set; } = null!;
    public string DescriptionVN { get; set; } = null!;
    public string DescriptionEN { get; set; } = null!;
    public DateTime CreateAt { get; set; }
    public Guid CreateBy { get; set; }
    public string? Creator { get; set; }
    public DateTime UpdateAt { get; set; }
    public Guid UpdateBy { get; set; }
    public string? Updater { get; set; }
}
