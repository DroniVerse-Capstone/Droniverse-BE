using Droniverse.Community.Application.States.RoundState;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Services
{
    public class RoundLifecycleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoundStateFactory _stateFactory;
        private readonly IClock _clock;

        public RoundLifecycleService(
            IUnitOfWork unitOfWork,
            IRoundStateFactory stateFactory,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _stateFactory = stateFactory;
            _clock = clock;
        }

        public async Task UpdateRoundStatusesAsync()
        {
            var now = _clock.Now;

            var rounds = await _unitOfWork.Rounds.GetManyByCondition(
                r => r.Status != RoundStatus.Finished &&
                     r.Status != RoundStatus.SCHEDULE_INVALID,
                q => q.Include(r => r.Competition)
            );

            bool isModified = false;

            foreach (var round in rounds)
            {
                var state = _stateFactory.GetState(round.Status);

                var oldStatus = round.Status;

                bool isPreviousRoundFinished = true;

                if (round.RoundNumber > 1)
                {
                    var prevStatus = await _unitOfWork.Rounds
                        .GetManyByConditionAsQueryable(r =>
                            r.CompetitionID == round.CompetitionID &&
                            r.RoundNumber == round.RoundNumber - 1,
                            q => q.AsNoTracking())
                        .Select(r => (RoundStatus?)r.Status)
                        .FirstOrDefaultAsync();

                    isPreviousRoundFinished = prevStatus == RoundStatus.Finished;
                }

                state.Handle(round, now, isPreviousRoundFinished);

                if (round.Status != oldStatus)
                    isModified = true;
                // 👉 chỗ này bạn có thể thêm logic sau:
                // - gửi notification khi round start
                // - gửi notification khi round finish
                // - trigger scoring
            }

            if (isModified)
                await _unitOfWork.SaveChangeAsync();

        }
    }
}
