using Droniverse.Community.Application.Services;
using Droniverse.Shared.Services;
using Hangfire;
using System.Diagnostics;

namespace Droniverse.Community.Application.Jobs
{
    [AutomaticRetry(Attempts = 3)]
    public class CompetitionStatusJob
    {
        private readonly CompetitionLifecycleService _competitionLifecycleService;
        private readonly RoundLifecycleService _roundLifecycleService;
        private readonly ILogger<CompetitionStatusJob> _logger;
        private readonly IClock _clock;

        public CompetitionStatusJob(
            CompetitionLifecycleService competitionLifecycleService,
            RoundLifecycleService roundLifecycleService,
            ILogger<CompetitionStatusJob> logger,
            IClock clock)
        {
            _competitionLifecycleService = competitionLifecycleService;
            _roundLifecycleService = roundLifecycleService;
            _logger = logger;
            _clock = clock;
        }

        [JobDisplayName("Competition + Round Lifecycle Job")]
        [DisableConcurrentExecution(10)]
        [Queue("critical")]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("CompetitionStatusJob (combined) started");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _competitionLifecycleService.UpdateCompetitionStatusesAsync();
                await _roundLifecycleService.UpdateRoundStatusesAsync();

                stopwatch.Stop();
                var finishedAt = _clock.Now;

                _logger.LogInformation(
                    "CompetitionStatusJob (combined) finished in {ElapsedMs} ms at {FinishedAt}",
                    stopwatch.ElapsedMilliseconds,
                    finishedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CompetitionStatusJob (combined) failed");
                throw;
            }
        }
    }
}
