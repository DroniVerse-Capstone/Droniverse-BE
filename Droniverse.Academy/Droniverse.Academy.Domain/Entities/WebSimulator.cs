using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;

public class WebSimulator
{
    public Guid WebSimulatorID { get; set; }
    public string TitleEN { get; set; } = string.Empty;
    public string TitleVN { get; set; } = string.Empty;
    public WebSimulatorType Type { get; set; }
    public string ObjectivesVN { get; set; } = string.Empty;
    public string ObjectivesEN { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
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
