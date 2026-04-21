namespace Droniverse.Academy.Application.DTO.Response;

public class ReportResponseDTO
{
    public Guid ReportID { get; set; }
    public Guid ReferenceID { get; set; }
    public Guid UserID { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ResponseVN { get; set; }
    public string? ResponseEN { get; set; }
}
