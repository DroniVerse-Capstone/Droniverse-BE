using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.Services;
using Droniverse.Shared.Services;
using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Droniverse.Community.Application.Jobs
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public class CompetitionStatusJob
    {
        private readonly CompetitionLifecycleService _lifecycleService;
        private readonly ILogger<CompetitionStatusJob> _logger;
        //private readonly IClock _clock;

        public CompetitionStatusJob(
            CompetitionLifecycleService lifecycleService,
            ILogger<CompetitionStatusJob> logger)
        {
            _lifecycleService = lifecycleService;
            _logger = logger;
        }

        [JobDisplayName("Competition Lifecycle Job")]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("CompetitionStatusJob started");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _lifecycleService.UpdateCompetitionStatusesAsync();

                stopwatch.Stop();
                _logger.LogInformation($"Executed in {stopwatch.ElapsedMilliseconds} ms");
                _logger.LogInformation("CompetitionStatusJob finished successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CompetitionStatusJob failed");
                throw;
            }
        }
    }
}
