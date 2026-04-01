using Droniverse.Community.Application.Services;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Droniverse.Community.Application.Jobs
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(300)]
    public class RoundStatusJob
    {
        private readonly RoundLifecycleService _lifecycleService;
        private readonly ILogger<RoundStatusJob> _logger;

        public RoundStatusJob(
            RoundLifecycleService lifecycleService,
            ILogger<RoundStatusJob> logger)
        {
            _lifecycleService = lifecycleService;
            _logger = logger;
        }

        [JobDisplayName("Round Lifecycle Job")]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("RoundStatusJob started");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _lifecycleService.UpdateRoundStatusesAsync();

                stopwatch.Stop();
                _logger.LogInformation($"Executed in {stopwatch.ElapsedMilliseconds} ms");
                _logger.LogInformation("RoundStatusJob finished successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoundStatusJob failed");
                throw;
            }
        }
    }
}