using Droniverse.Community.Application.IService;
using Droniverse.Shared.Services;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Droniverse.Community.API.Jobs
{
    [AutomaticRetry(Attempts = 3)]
    public class HotCompetitionsJob
    {
        private readonly ICompetitionService _competitionService;
        private readonly ILogger<HotCompetitionsJob> _logger;
        private readonly IClock _clock;

        public HotCompetitionsJob(
            ICompetitionService competitionService,
            ILogger<HotCompetitionsJob> logger,
            IClock clock)
        {
            _competitionService = competitionService;
            _logger = logger;
            _clock = clock;
        }

        [JobDisplayName("Hot Competitions Cache Refresh Job")]
        [DisableConcurrentExecution(10)]
        [Queue("default")]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("HotCompetitionsJob bắt đầu");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _competitionService.RefreshHotCompetitionsCacheAsync();

                stopwatch.Stop();
                var finishedAt = _clock.Now;
                _logger.LogInformation(
                    "HotCompetitionsJob đã hoàn thành {ElapsedMs} ms at {FinishedAt}",
                    stopwatch.ElapsedMilliseconds,
                    finishedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HotCompetitionsJob bị lỗi");
                throw;
            }
        }
    }
}
