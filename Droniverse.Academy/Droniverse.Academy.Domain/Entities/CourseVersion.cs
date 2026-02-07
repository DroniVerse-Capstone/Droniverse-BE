using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;

public class CourseVersion
{
    public Guid CourseVersionID { get; set; }        // PK

    public Guid CourseID { get; set; }               // FK -> Course
    public Course Course { get; set; }

    public string TitleVN { get; set; }              // varchar(255)
    public string TitleEN { get; set; }              // varchar(255)
    public string DescriptionVN { get; set; }        // text
    public string DescriptionEN { get; set; }        // text

    public CourseVersionStatus Status { get; set; }  // tinyint
    public int Version { get; set; }                 // version number

    public string ImageUrl { get; set; }             // text

    public CourseLevel Level { get; set; }            // varchar(10)
    public int EstimatedDuration { get; set; }       // minutes

    public Guid UpdateBy { get; set; }                // FK -> User
    public DateTime UpdateAt { get; set; }

    // Navigation
    public ICollection<Module> Modules { get; set; }
    public ICollection<CourseVersionCategory> CourseVersionCategories { get; set; }
    public ICollection<Code> Codes { get; set; }
    public ICollection<Feedback> Feedbacks { get; set; }
    public ICollection<RequiredDrone> RequiredDrones { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; }

    public Certificate Certificate { get; set; }     // 1–1
}
