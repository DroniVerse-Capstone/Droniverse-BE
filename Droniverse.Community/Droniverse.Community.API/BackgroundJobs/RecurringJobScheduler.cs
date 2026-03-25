using Droniverse.Community.Application.Job;
using Droniverse.Community.Application.Jobs;
using Hangfire;

namespace Droniverse.Community.API.BackgroundJobs
{
    public class RecurringJobScheduler
    {
        private const string CompetitionStatusJobId = "update-competition-status";
        private const string TestJob = "test-job";
        private const string TestJob2 = "test-job2";

   
        public static void ScheduleJobs()
        {
            // Remove old persisted metadata first to avoid type resolution errors
            // when the job type was changed/moved between builds.
            RecurringJob.RemoveIfExists(CompetitionStatusJobId);

            // Competition status job
            RecurringJob.AddOrUpdate<CompetitionStatusJob>(
                CompetitionStatusJobId,
                job => job.ExecuteAsync(),
                Cron.Minutely
            );
            //RecurringJob.AddOrUpdate<TestJob>(
            //    TestJob,
            //    job => job.ExecuteAsync(),
            //    Cron.Minutely
            //);  
            //RecurringJob.AddOrUpdate<TestJob2>(
            //    TestJob2,
            //    job => job.ExecuteAsync(),
            //    Cron.Minutely
            //);  
            // TODO: Thêm các job khác ở đây
            // RecurringJob.AddOrUpdate<AnotherJob>(
            //    "another-job",
            //    job => job.ExecuteAsync(),
            //    Cron.Hourly
            // );
        }
    }
}
