using Droniverse.Identity.Domain.Enums;

namespace Droniverse.Identity.Domain.Entities;

public class SysPolicy
{
    public Guid SysPolicyID { get; set; }
    public SysPolicyType Type { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Account CreatedByUser { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy
    {
        get; set;
    }
    public Account UpdatedByUser { get; set; }
}

