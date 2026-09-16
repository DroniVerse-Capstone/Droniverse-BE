using Droniverse.Community.Application.Services;
using Droniverse.Shared.Services;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Droniverse.Community.Application.Jobs
{
    [AutomaticRetry(Attempts = 3)]
    public class RoundStatusJob
    {
        private readonly RoundLifecycleService _lifecycleService;
        private readonly ILogger<RoundStatusJob> _logger;
        private readonly IClock _clock;

        public RoundStatusJob(
            RoundLifecycleService lifecycleService,
            ILogger<RoundStatusJob> logger,
            IClock clock)
        {
            _lifecycleService = lifecycleService;
            _logger = logger;
            _clock = clock;
        }

        [JobDisplayName("Round Lifecycle Job")]
        [DisableConcurrentExecution(10)]
        [Queue("critical")]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("RoundStatusJob started");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _lifecycleService.UpdateRoundStatusesAsync();

                stopwatch.Stop();
                var finishedAt = _clock.Now;
                _logger.LogInformation(
                    "RoundStatusJob finished in {ElapsedMs} ms at {FinishedAt}",
                    stopwatch.ElapsedMilliseconds,
                    finishedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoundStatusJob failed");
                throw;
            }
        }
    }
}