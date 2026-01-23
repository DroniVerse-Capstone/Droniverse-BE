namespace Droniverse.Academy.Domain.Entities;
public class Module
{
    public Guid ModuleID { get; set; }
    public CourseVersion CourseVersion { get; set; }
    public Guid CourseID { get; set; }
    public ICollection<UserModule> UserModules { get; set; }
    public ICollection<Lesson> Lessons { get; set; }

    public string TitleVN { get; set; } //varchar(255)
    public string TitleEN { get; set; } //varchar(255)
    public int ModuleNumber { get; set; } // int
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
}
