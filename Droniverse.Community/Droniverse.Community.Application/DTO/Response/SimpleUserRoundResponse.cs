using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record SimpleUserRoundResponse
    {
        public UserRoundStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public decimal? Point { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
        public bool? IsPassed { get; set; }
        public int? Rank { get; set; }
    }
}
