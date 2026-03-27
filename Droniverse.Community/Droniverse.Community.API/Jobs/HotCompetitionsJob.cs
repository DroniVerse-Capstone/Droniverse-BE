using Droniverse.Community.Application.IService;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Droniverse.Community.API.Jobs
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public class HotCompetitionsJob
    {
        private readonly ICompetitionService _competitionService;
        private readonly ILogger<HotCompetitionsJob> _logger;

        public HotCompetitionsJob(
            ICompetitionService competitionService,
            ILogger<HotCompetitionsJob> logger)
        {
            _competitionService = competitionService;
            _logger = logger;
        }

        [JobDisplayName("Hot Competitions Cache Refresh Job")]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("HotCompetitionsJob bắt đầu");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _competitionService.RefreshHotCompetitionsCacheAsync();

                stopwatch.Stop();
                _logger.LogInformation("HotCompetitionsJob được thực thi trong {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
                _logger.LogInformation("HotCompetitionsJob đã hoàn thành");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HotCompetitionsJob bị lỗi");
                throw;
            }
        }
    }
}
