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
            if (competition.CanAutoOpenRegistration(now))
            {
                competition.SystemOpenRegistration(now);
            }
        }
    }
}
