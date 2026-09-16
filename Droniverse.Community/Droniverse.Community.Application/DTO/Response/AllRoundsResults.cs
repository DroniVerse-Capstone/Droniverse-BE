using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record RoundResultsDto
    {
        public required SimpleRoundResponse RoundInfo { get; set; }
        public required PaginationResult<IEnumerable<ParticipantResultResponse>> UserResults { get; set; }
    }

    public record ParticipantResultResponse
    {
        public required SimpleUserReponse UserInfo { get; set; }
        public required SimpleUserRoundResponse ParticipantResult { get; set; }
    }
}
