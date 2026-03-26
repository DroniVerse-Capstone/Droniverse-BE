using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.CompetitionState
{
    public class OngoingState : ICompetitionState
    {
        public CompetitionStatus Status => CompetitionStatus.ONGOING;

        public void Handle(Competition competition, DateTime now)
        {
            if (competition.CanAutoFinishCompetition(now))
            {
                competition.SystemFinishCompetition(now);
            }
        }
    }
}
