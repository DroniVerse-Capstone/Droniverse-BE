using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Code
{
    public string CodeID { get; set; } //char(36)
    public Course Course { get; set; }
    public Guid CourseID { get; set; } //char(36)
    public ICollection<CodeUsage> CodeUsages { get; set; }
    public DateTime ExpireDate { get; set; }
    public CodeStatus Status { get; set; }
}
