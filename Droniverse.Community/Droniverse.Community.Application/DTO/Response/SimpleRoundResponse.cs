using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record SimpleRoundResponse
    {
        public Guid RoundId { get; init; }
        public int RoundNumber { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public TimeSpan TimeLimit { get; init; }
        public RoundStatus RoundStatus { get; init; }
    }
}
