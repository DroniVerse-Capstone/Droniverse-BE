namespace Droniverse.Academy.Domain.Entities;

public class StructureSimulator
{
    public Guid StructureID { get; set; }
    public string? ContentVN { get; set; }
    public Guid CreateBy { get; private set; }
    public Guid UpdateBy { get; private set; }
    public DateTime CreateAt { get; private set; }
    public DateTime UpdateAt { get; private set; }
    public int EstimatedTime { get; set; }

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
