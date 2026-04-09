using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Community.Domain.AppHelpers
{
    public static class CommunityAppHelpers
    {
        public static CompetitionLifeCycleStatus? GetCurrentCompetitionLifeCycle(
            Competition competition,
            DateTime now)
        {
            if (competition == null)
                throw new ArgumentNullException(nameof(competition));

            if (competition.Status != CompetitionStatus.PUBLISHED && competition.Status != CompetitionStatus.RESULT_PUBLISHED)
                return null;

            if (now < competition.VisibleAt)
                return CompetitionLifeCycleStatus.UPCOMING;

            if (now <= competition.RegistrationStartDate)
                return CompetitionLifeCycleStatus.COMING_SOON;

            if (now < competition.RegistrationEndDate)
                return CompetitionLifeCycleStatus.REGISTRATION_OPEN;

            if (now < competition.StartDate)
                return CompetitionLifeCycleStatus.REGISTRATION_CLOSED;

            if (now <= competition.EndDate)
                return CompetitionLifeCycleStatus.ONGOING;

            return CompetitionLifeCycleStatus.FINISHED;
        }

        public static RoundLifeCycleStatus? GetCurrentRoundLifeCycle(
          RoundStatus roundStatus,
          DateTime startTime,
          DateTime endTime,
          DateTime now)
        {

            if (roundStatus != RoundStatus.Valid)
                return null;

            if (now < startTime)
                return RoundLifeCycleStatus.Upcoming;

            if (now <= endTime)
                return RoundLifeCycleStatus.Ongoing;

            return RoundLifeCycleStatus.Finished;
        }
    }
}
