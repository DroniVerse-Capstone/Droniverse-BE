using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record RoundParticipantsEntryResponse
    {
        public required SimpleUserReponse User { get; set; }
        public DateTime StartedAt { get; set; }
        public UserRoundStatus Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool? IsPassed { get; set; }
    }
}
