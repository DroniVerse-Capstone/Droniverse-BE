using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Lab
{
    public Guid LabID { get; set; } //char(36)
    public ICollection<UserLab> UserLabs { get; set; }
    public ICollection<Report> Reports { get; set; }
    public LabType Type { get; set; } //varchar(50)
    public LabLevel Level { get; set; } = LabLevel.EASY; //tinyint [0: EASY, 1: MEDIUM, 2: HARD]
    public LabStatus Status { get; set; } = LabStatus.DRAFT; //tinyint [0: DRAFT, 1: ACTIVE, 2: INACTIVE]
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


}
