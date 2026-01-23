using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;
public class Club
{
    public Guid ClubID { get; set; } //char(36)

    public Guid CreateBy { get; set; } //char(36) // reference to UserID
    public ICollection<ClubCategory> ClubCategories { get; set; }
    public ICollection<ClubCourse> ClubCourses { get; set; }
    public ICollection<Participation> Participations { get; set; }
    public ICollection<Competition> Competitions { get; set; }
    public ICollection<ClubRequest> ClubRequests { get; set; }

    public string NameVN { get; set; } //varchar(255)
    public string NameEN { get; set; } //varchar(255)
    public string DescriptionVN { get; set; } //text
    public string DescriptionEN { get; set; } //text
    public Guid ClubCode { get; set; } //char(6)
    public ClubStatus Status  { get; set; } //bit
    public bool IsPublic { get; set; } //boolean
    public int LimitParticipation { get; set; } //int
    public int LimitClubManagers { get; set; }

}
