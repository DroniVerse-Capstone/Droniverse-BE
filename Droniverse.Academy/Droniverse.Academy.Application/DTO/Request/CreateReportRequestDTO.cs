namespace Droniverse.Academy.Application.DTO.Request;

public class CreateReportRequestDTO
{
    public Guid LabID { get; set; }
    public string Content { get; set; } = string.Empty;
}
