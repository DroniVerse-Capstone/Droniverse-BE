using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.CompetitionState
{
    public class CancelledState : ICompetitionState
    {
        public CompetitionStatus Status => CompetitionStatus.CANCELLED;

        public void Handle(Competition competition, DateTime now)
        {
            // nothing to do
        }
    }
}
