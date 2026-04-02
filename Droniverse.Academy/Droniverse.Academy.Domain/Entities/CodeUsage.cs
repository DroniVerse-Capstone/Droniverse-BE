namespace Droniverse.Academy.Domain.Entities;
public class CodeUsage
{
    public Code Code { get; set; }
    public string CodeID { get; set; } //char(36)
    public Guid UserID { get; set; } //char(36)
    public DateTime UsedDate { get; set; }
}
