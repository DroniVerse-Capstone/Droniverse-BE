using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public class RoundLeaderBoardResponse
    {
        public Guid RoundID { get; set; }
        public required List<RoundLeaderboardEntryDto> roundEntries { get; set; }
    }
}
