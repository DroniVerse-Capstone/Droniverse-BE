namespace Droniverse.Academy.Domain.Entities;

public class Assignment
{
    public Guid AssignmentID { get; set; }
    public string TitleEN { get; set; } = string.Empty;
    public string TitleVN { get; set; } = string.Empty;
    public string DescriptionEN { get; set; } = string.Empty;
    public string DescriptionVN { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;
    public int EstimatedTime { get; set; }
    public Guid CreateBy { get; private set; }
    public Guid UpdateBy { get; private set; }
    public DateTime CreateAt { get; private set; }
    public DateTime UpdateAt { get; private set; }

    public ICollection<UserAssignment> UserAssignments { get; set; } = [];

    public void SetAuditOnCreate(Guid userId, DateTime now)
    {
        CreateBy = userId;
        UpdateBy = userId;
        CreateAt = now;
        UpdateAt = now;
    }

    public void SetAuditOnUpdate(Guid userId, DateTime now)
    {
        UpdateBy = userId;
        UpdateAt = now;
    }
}
