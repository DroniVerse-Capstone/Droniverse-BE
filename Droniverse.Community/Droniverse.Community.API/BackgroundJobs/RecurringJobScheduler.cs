using Droniverse.Community.Application.Jobs;
using Hangfire;

namespace Droniverse.Community.API.BackgroundJobs
{
    public static class RecurringJobScheduler
    {
        private const string CompetitionStatusJobId = "competition-lifecycle-job";
        private const string RoundStatusJobId = "round-lifecycle-job";

        public static void ScheduleJobs()
        {
            ScheduleCompetitionJob();
            ScheduleRoundJob();
        }

        private static void ScheduleCompetitionJob()
        {
            RecurringJob.AddOrUpdate<CompetitionStatusJob>(
                CompetitionStatusJobId,
                job => job.ExecuteAsync(),
                Cron.Minutely,
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                }
            );
        }

        private static void ScheduleRoundJob()
        {
            RecurringJob.AddOrUpdate<RoundStatusJob>(
                RoundStatusJobId,
                job => job.ExecuteAsync(),
                Cron.Minutely,
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                }
            );
        }
    }
}