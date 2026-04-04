using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public class RoundJoinResponse
    {
        public Guid UserRoundId { get; set; }
        public DateTime StartedAt { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime DeadlineAt { get; set; }
        public DateTime ServerTime { get; set; }
        public UserRoundStatus Status { get; set; }
        public int RemainingSeconds { get; set; }
    }
}
