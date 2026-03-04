using Droniverse.Community.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Domain.Entities;
public class Club
{
    public Guid ClubID { get; set; } //char(36)
    public Guid CreateBy { get; set; } //char(36) // reference to UserID
    public ICollection<ClubCategory> ClubCategories { get; set; }
    public ICollection<ClubCourse> ClubCourses { get; set; }
    public ICollection<Participation> Participations { get; set; }
    public ICollection<Competition> Competitions { get; set; }
    public ICollection<ClubAttemptRequest> ClubRequests { get; set; }
    [Length(1,255, ErrorMessage = "Club name must be between 1 to 255 characters !")]
    public string NameVN { get; set; } //varchar(255)
    [Length(1, 255, ErrorMessage = "Club name must be between 1 to 255 characters !")]
    public string NameEN { get; set; } //varchar(255)
    public string DescriptionVN { get; set; }   
    public string DescriptionEN { get; set; } 
    [Required]
    [Length(6,6, ErrorMessage = "Club code must be 6 characters !")]
    public string ClubCode { get; set; }
    [Required]
    public ClubStatus Status  { get; set; } // 0: INACTIVE; 1: ACTIVE
    [Required]
    public bool IsPublic { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Club must have at least 1 member !")]
    public int LimitParticipation { get; set; } 
    [Range(1, 10, ErrorMessage = "Club just has from 1 to 10 manager")]
    public int LimitClubManagers { get; set; }

}
