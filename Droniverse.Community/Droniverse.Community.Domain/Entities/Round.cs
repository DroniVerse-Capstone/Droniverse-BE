using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;

public class Round
{
    public Guid RoundID { get; set; }
    public Competition Competition { get; set; }
    public Guid CompetitionID { get; set; }
    public Guid LabID { get; set; }
    public ICollection<UserRound>? UserRounds { get; set; }
    public int RoundNumber { get; set; }
    public RoundStatus Status { get; set; }
}

