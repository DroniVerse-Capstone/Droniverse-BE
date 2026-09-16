using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.DTO.Response
{
    public class UserCompetitionResponseDto
    {
        public Guid UserCompetitionID { get; set; }
        public required SimpleUserReponse User { get; set; }
        public required SimpleCompetitionResponse Competition { get; set; }
        public UserCompetitionStatus Status { get; set; }
        public decimal? Score { get; set; }
        public int? Rank { get; set; }
        public Guid? PrizeID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
