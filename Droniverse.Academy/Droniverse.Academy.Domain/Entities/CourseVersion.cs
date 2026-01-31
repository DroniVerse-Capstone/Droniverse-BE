using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class CourseVersion
{
    public Guid CourseVersionID { get; set; }

    public Course Course { get; set; }
    public Guid CourseID { get; set; }
    public ICollection<Module> Modules { get; set; }
    public ICollection<CourseVersionCategory> CourseVersionCategories { get; set; }
    public ICollection<Code> Codes { get; set; }
    public ICollection<Feedback> Feedbacks { get; set; }
    public ICollection<RequiredDrone> RequiredDrones { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; }
    public Guid UpdateBy { get; set; } // reference to UserID

    public string TitleVN { get; set; } //varchar(255)
    public string TitleEN { get; set; } //varchar(255)
    public string DescriptionVN { get; set; } //text
    public string DescriptionEN { get; set; } //text
    public CourseStatus Status { get; set; } = CourseStatus.ACTIVE; //bit, ACTIVE, INACTIVE
    public int Version { get; set; } = 1;
    public string ImageUrl { get; set; } //varchar(255)
    public CourseLevel Level { get; set; } = CourseLevel.EASY; //varchar(10) EASY, MEDIUM, HARD
    public int EstimatedDuration { get; set; }
    public DateTime UpdateAt { get; set; }

}
