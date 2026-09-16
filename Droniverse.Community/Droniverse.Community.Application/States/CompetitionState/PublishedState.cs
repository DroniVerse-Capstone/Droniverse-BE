using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.CompetitionState
{
    public class PublishedState : ICompetitionState
    {
        public CompetitionStatus Status => CompetitionStatus.PUBLISHED;

        public void Handle(Competition competition, DateTime now)
        {
            if (competition.CanAutoInvalidCompetition(now))
            {
                competition.SystemInvalidCompetition(CompetitionInvalidReason.NoRounds, now);
                return;
            }

            if (competition.CanAutoOpenRegistration(now))
            {
                competition.SystemOpenRegistration(now);
            }

            if (competition.CanAutoCloseRegistration(now))
            {
                competition.SystemCloseRegistration(now);
            }

            if (competition.CanAutoStartCompetition(now))
            {
                competition.SystemStartCompetition(now);
            }

            if (competition.CanAutoFinishCompetition(now))
            {
                competition.SystemFinishCompetition(now);
            }
        }
    }
}
