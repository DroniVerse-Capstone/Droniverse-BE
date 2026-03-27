namespace Droniverse.Academy.Domain.Entities;

public class Theory
{
    public Guid TheoryID { get; set; } //char(36)
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public string ContentVN { get; set; } //text
    public string ContentEN { get; set; } //text
    public Guid CreateBy { get; private set; } //char(36) // reference to User
    public Guid UpdateBy { get; private set; }
    public DateTime CreateAt { get; private set; }
    public DateTime UpdateAt { get; private set; }
    public int EstimatedTime { get; set; }

    public void SetAuditOnCreate(Guid userId, DateTime now)
    {
        CreateBy = userId;
        CreateAt = now;
        UpdateBy = userId;
        UpdateAt = now;
    }

    public void SetAuditOnUpdate(Guid userId, DateTime now)
    {
        UpdateBy = userId;
        UpdateAt = now;
    }
}
