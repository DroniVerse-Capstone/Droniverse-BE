using Droniverse.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs.Request
{
    public record GetClubParticipantIdsRequest
    {
        public ParticipationStatus ParticipantStatus { get; set; } = ParticipationStatus.ACTIVE;
    }
}
