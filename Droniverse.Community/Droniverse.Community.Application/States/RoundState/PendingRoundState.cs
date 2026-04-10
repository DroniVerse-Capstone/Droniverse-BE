using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.States.RoundState
{
    public class PendingRoundState : IRoundState
    {
        public RoundStatus Status => RoundStatus.Valid;

        public void Handle(Round round, DateTime now, bool isPreviousRoundFinished)
        {
            if (round.IsScheduleInvalid(now))
            {
                round.MarkAsScheduleInvalid();
                return;
            }

            // RoundStatus hiện chỉ phản ánh tính hợp lệ nghiệp vụ.
            // Lifecycle đang được xử lý theo timeline ở service.
        }
    }
}