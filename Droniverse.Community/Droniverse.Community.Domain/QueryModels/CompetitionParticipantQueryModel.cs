using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Infrastructure.QueryModels
{
    public class CompetitionParticipantQueryModel
    {
        public Guid UserId { get; set; }
        public UserCompetitionStatus Status { get; set; }
        public decimal? Score { get; set; }
        public int? Rank { get; set; }
        public Guid? PrizeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
