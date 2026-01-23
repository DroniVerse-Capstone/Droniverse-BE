using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Code
{
    public Guid CodeID { get; set; } //char(36)
    
    public CourseVersion CourseVersion { get; set; }
    public Guid CourseVersionID { get; set; } //char(36)
    public ICollection<CodeUsage> CodeUsages { get; set; }
    public DateTime ExpireDate { get; set; }
    public CodeStatus Status { get; set; }
}
