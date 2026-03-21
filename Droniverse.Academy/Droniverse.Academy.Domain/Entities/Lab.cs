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
    public Guid CreateBy { get; set; } // reference to UserID
    public Guid UpdateBy { get; set; }
    public DateTime CreateAt { get; set;}
    public DateTime UpdateAt { get; set; }


}
