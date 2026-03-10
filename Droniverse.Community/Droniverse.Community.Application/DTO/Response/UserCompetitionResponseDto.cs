using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class UserCompetitionResponseDto
    {
        public Guid UserCompetitionID { get; set; }
        public Guid UserID { get; set; }
        public Guid CompetitionID { get; set; }
        public UserCompetitionStatus Status { get; set; }
        public decimal? Score { get; set; }
        public int? Rank { get; set; }
        public Guid? PrizeID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
