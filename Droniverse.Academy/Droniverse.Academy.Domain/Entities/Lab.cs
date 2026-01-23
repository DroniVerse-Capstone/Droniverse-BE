using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class Lab
{
    public Guid LabID { get; set; } //char(36)
    public Lesson Lesson { get; set; }
    public Guid LessonID { get; set; } //char(36)
    public ICollection<UserLab> UserLabs { get; set; }
    public ICollection<Report> Reports { get; set; }
    public LabType Type { get; set; } //varchar(50)
    public string NameVN { get; set; } //nvarchar(255)
    public string NameEN { get; set; } //nvarchar(255)
    public string DescriptionVN { get; set; } //text
    public string DescriptionEN { get; set; } //text
    public int Version { get; set; } = 1;
    public Guid CreateBy { get; set; } // reference to UserID
    public Guid UpdateBy { get; set; }
    public DateTime CreateAt { get; set;}
    public DateTime UpdateAt { get; set; }


}
