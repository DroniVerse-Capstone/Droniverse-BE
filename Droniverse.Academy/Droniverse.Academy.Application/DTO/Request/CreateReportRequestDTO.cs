using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class CreateReportRequestDTO
{
    public Guid ReferenceID { get; set; }
    public ReportType ReportType { get; set; }
    public string? ContentVN { get; set; }
    public string? ContentEN { get; set; }
}
