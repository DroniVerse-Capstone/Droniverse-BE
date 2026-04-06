using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record MyRoundsResultResponse
    {
        public required SimpleRoundResponse RoundInfo { get; set; }
        public required SimpleUserRoundResponse UserRoundResult { get; set; }
    }
}
