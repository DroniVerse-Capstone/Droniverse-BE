using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;
public class ClubCourse
{
    public Club Club { get; set; }
    public Guid ClubID { get; set; }
    public Guid CourseID { get; set; } // Reference to Course entity (not defined here)

    public ClubCourseProfit isProfit { get; set; }
}

