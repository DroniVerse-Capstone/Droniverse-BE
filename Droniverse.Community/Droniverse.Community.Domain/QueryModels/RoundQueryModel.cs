using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Infrastructure.QueryModels
{
    public class RoundQueryModel
    {
        public Guid RoundID { get; set; }
        public Guid CompetitionID { get; set; }
        public string NameVN { get; set; } = null!;
        public string NameEN { get; set; } = null!;
        public Guid LabID { get; set; }
        public int RoundNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public RoundStatus Status { get; set; }
        public int TotalParticipants { get; set; }
    }
}
