namespace Droniverse.Community.Domain.Entities;
public class Competition
{
    public Guid CompetitionID { get; set; } //char(36)
    public ICollection<CompetitionCertificate> CompetitionCertificates { get; set; }
    public ICollection<UserCompetion> UserCompetitions { get; set; }
    public ICollection<Round> Rounds { get; set; }
    public Club Club { get; set; }
    public Guid ClubID { get; set; } // Reference to ClubID, char(36)
    public Guid CreateBy { get; set; } // Reference to UserID, char(36)
    public string NameVN { get; set; } // varchar(255)
    public string NameEN { get; set; } // varchar(255)
    public string DescriptionVN { get; set; } // text
    public string DescriptionEN { get; set; } // text
    public DateTime StartDate { get; set; } // datetime
    public DateTime EndDate { get; set; } // datetime
}
