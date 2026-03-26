using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.States.RoundState
{
    public class PendingRoundState : IRoundState
    {
        public RoundStatus Status => RoundStatus.Pending;

        public void Handle(Round round, DateTime now, bool isPreviousRoundFinished)
        {
            if (round.IsScheduleInvalid(now))
            {
                round.MarkAsScheduleInvalid();
                return;
            }

            var competitionStatus = round.Competition?.Status ?? CompetitionStatus.DRAFT;

            if (round.CanStart(now, isPreviousRoundFinished, competitionStatus))
            {
                round.StartRound(now);
            }
        }
    }
}