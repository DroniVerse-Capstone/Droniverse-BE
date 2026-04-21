namespace Droniverse.Academy.Application.DTO.Request;

public class CreateReportRequestDTO
{
    public Guid ReferenceID { get; set; }
    public string Content { get; set; } = string.Empty;
}
