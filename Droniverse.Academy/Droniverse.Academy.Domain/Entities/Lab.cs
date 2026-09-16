using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Domain.Entities;
public class Lab
{
    public Guid LabID { get; set; } //char(36)
    public ICollection<UserLab> UserLabs { get; set; }
    public LabType Type { get; set; } //varchar(50)
    public LabLevel Level { get; set; } = LabLevel.EASY; //tinyint [0: EASY, 1: MEDIUM, 2: HARD]
    public LabStatus Status { get; private set; } = LabStatus.DRAFT; //tinyint [0: DRAFT, 1: ACTIVE, 2: INACTIVE]
    public string NameVN { get; set; } //nvarchar(255)
    public string NameEN { get; set; } //nvarchar(255)
    public string DescriptionVN { get; set; } //text
    public string DescriptionEN { get; set; } //text
    public int EstimatedTime { get; set; }
    public Guid CreateBy { get; private set; } // reference to UserID
    public Guid UpdateBy { get; private set; }
    public DateTime CreateAt { get; private set;}
    public DateTime UpdateAt { get; private set; }

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

    public void Activate()
    {
        if (Status != LabStatus.DRAFT && Status != LabStatus.INACTIVE && Status != LabStatus.LOCKED)
            throw new DomainException($"Cannot activate lab from status {Status}");

        Status = LabStatus.ACTIVE;
    }

    public void Inactivate()
    {
        if (Status != LabStatus.ACTIVE && Status != LabStatus.LOCKED)
            throw new DomainException($"Cannot inactivate lab from status {Status}");

        Status = LabStatus.INACTIVE;
    }

    public void Lock()
    {
        if (Status != LabStatus.ACTIVE && Status != LabStatus.INACTIVE)
            throw new DomainException($"Cannot lock lab from status {Status}");

        Status = LabStatus.LOCKED;
    }

    public void Delete()
    {
        if (Status == LabStatus.DELETED)
            throw new DomainException("Lab is already deleted.");

        Status = LabStatus.DELETED;
    }

    public void ResetToDraft()
    {
        Status = LabStatus.DRAFT;
    }

    public void TransitionTo(LabStatus targetStatus)
    {
        if (Status == targetStatus)
            return;

        switch (targetStatus)
        {
            case LabStatus.DRAFT:
                ResetToDraft();
                break;
            case LabStatus.ACTIVE:
                Activate();
                break;
            case LabStatus.INACTIVE:
                Inactivate();
                break;
            case LabStatus.LOCKED:
                Lock();
                break;
            case LabStatus.DELETED:
                Delete();
                break;
            default:
                throw new DomainException($"Unsupported lab status {targetStatus}");
        }
    }


}
