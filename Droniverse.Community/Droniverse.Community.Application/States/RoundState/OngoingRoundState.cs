using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.RoundState
{
    public class OngoingRoundState : IRoundState
    {
        public RoundStatus Status => RoundStatus.Ongoing;

        public void Handle(Round round, DateTime now, bool isPreviousRoundFinished)
        {
            if (now >= round.EndTime)
            {
                round.FinishRound(now);
            }
        }
    }
}
