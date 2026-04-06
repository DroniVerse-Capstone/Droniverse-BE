using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;
public class ClubCourse
{
    public Club Club { get; private set; }
    public Guid ClubID { get; private set; }
    public Guid CourseID { get; private set; } // Reference to Course entity (not defined here)
    public ClubCourseProfit IsProfit { get; set; }
}

