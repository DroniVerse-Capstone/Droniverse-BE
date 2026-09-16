using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record DeletePrizeResponse
    {
        public int TotalDeleted { get; set; }
        public required IEnumerable<CompetitionPrizeResponseDto> RemainingPrizes { get; set; }
    }
}
