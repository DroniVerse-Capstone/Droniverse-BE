using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record CompetitionParticipantsResponse
    {
        public required SimpleCompetitionResponse Competition { get; set; }
        public UserCompetitionStatus ParticipantStatus { get; set; }
        public required PaginationResult<IEnumerable<CompetitionParticipantEntry>> participations { get; set; }
    }

    public record CompetitionParticipantEntry
    {
        public required SimpleUserReponse User { get; set; }
        public decimal? Score { get; set; }
        public int? Rank { get; set; }
        public Guid? PrizeID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
