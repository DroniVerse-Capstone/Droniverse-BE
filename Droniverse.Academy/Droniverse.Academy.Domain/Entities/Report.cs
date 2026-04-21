namespace Droniverse.Academy.Domain.Entities;
public class Report
{
    public Guid ReportID { get; set; } //char(36)
    
    public string Content { get; set; } //text
    public string ResponseVN { get; set; } //text
    public string ResponseEN { get; set; } //text
    public Guid ReferenceID { get; set; } //char(36)
    public Guid UserID { get; set; } //char(36)
}
