namespace Droniverse.Academy.Domain.Entities;

public class Theory
{
    public Guid TheoryID { get; set; } //char(36)
    public string ContentVN { get; set; } //text
    public string ContentEN { get; set; } //text
    public Guid CreateBy { get; set; } //char(36) // reference to User
    public Guid UpdateBy { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
    public int EstimatedTime { get; set; }
}
