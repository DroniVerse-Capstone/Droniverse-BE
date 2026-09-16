using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Report
{
    public Guid ReportID { get; set; } //char(36)
    public ReportType ReportType { get; set; } //char

    public string? ContentVN { get; set; } //text
    public string? ContentEN { get; set; } //text
    public string? ResponseVN { get; set; } //text
    public string? ResponseEN { get; set; } //text
    public Guid ReferenceID { get; set; } //char(36)
    public Guid UserID { get; set; } //char(36)
    public Guid? Responser { get; set; } //char(36)
}
