using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.States.CompetitionState
{
    public class RegistrationOpenState : ICompetitionState
    {
        public CompetitionStatus Status => CompetitionStatus.REGISTRATION_OPEN;

        public void Handle(Competition competition, DateTime now)
        {
            // 1. Nếu tới giờ thi → ưu tiên xử lý trước
            if (now >= competition.StartDate)
            {
                // Không có round → INVALID
                if (!competition.Rounds.Any(r => r.Status == RoundStatus.Pending))
                {
                    competition.SystemInvalidCompetition(CompetitionInvalidReason.NoRounds, now);
                    return;
                }

                // Có round → cố start
                try
                {
                    competition.SystemStartCompetition(now);
                    return;
                }
                catch
                {
                    competition.SystemInvalidCompetition(CompetitionInvalidReason.StartFailed, now);
                    return;
                }
            }

            // 2. Đóng đăng ký nếu tới hạn
            if (competition.CanAutoCloseRegistration(now))
            {
                competition.SystemCloseRegistration(now);
            }
        }
    }
}
