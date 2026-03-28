using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.CompetitionState
{
    public class RegistrationClosedState : ICompetitionState
    {
        public CompetitionStatus Status => CompetitionStatus.REGISTRATION_CLOSED;

        public void Handle(Competition competition, DateTime now)
        {
            if (now < competition.StartDate)
                return;

            // Không có round → INVALID
            if (!competition.Rounds.Any(r => r.Status == RoundStatus.Pending))
            {
                competition.SystemInvalidCompetition(CompetitionInvalidReason.NoRounds, now);
                return;
            }

            try
            {
                competition.SystemStartCompetition(now);
            }
            catch
            {
                competition.SystemInvalidCompetition(CompetitionInvalidReason.StartFailed, now);
            }
        }
    }
}
