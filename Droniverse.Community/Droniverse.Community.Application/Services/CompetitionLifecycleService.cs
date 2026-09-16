using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.States.CompetitionState;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class CompetitionLifecycleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompetitionStateFactory _stateFactory;
        private readonly IClock _clock;
        private readonly INotificationService _notificationService;

        public CompetitionLifecycleService(
            IUnitOfWork unitOfWork,
            ICompetitionStateFactory stateFactory,
            IClock clock,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _stateFactory = stateFactory;
            _clock = clock;
            _notificationService = notificationService;
        }

        public async Task UpdateCompetitionStatusesAsync()
        {
            var now = _clock.Now;

            var competitions = await _unitOfWork.Competitions.GetManyByCondition(
                c => c.Status != CompetitionStatus.CANCELLED &&
                     c.Status != CompetitionStatus.RESULT_PUBLISHED &&
                     c.Status != CompetitionStatus.INVALID,
                q => q.Include(c => c.Rounds)
            );

            bool isModified = false;

            foreach (var competition in competitions)
            {
                var state = _stateFactory.GetState(competition.Status);

                var oldStatus = competition.Status;

                state.Handle(competition, now);

                if (competition.Status != oldStatus)
                {

                    if (oldStatus != CompetitionStatus.INVALID  && competition.Status == CompetitionStatus.INVALID)
                        await _notificationService.SendCompetitionInvalidEmailAsync(competition);

                    isModified = true;
                }
            }

            if (isModified)
            {
                await _unitOfWork.SaveChangeAsync();
            }
        }
    }
}
