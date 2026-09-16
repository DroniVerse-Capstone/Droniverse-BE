using Droniverse.Academy.Application.Services;
using Droniverse.Shared.Services.IServices;
using Hangfire;
using System.Diagnostics;

namespace Droniverse.Academy.API.Jobs;

/// <summary>
/// Background job to expire codes with past expireDate
/// Runs daily at 2 AM (off-peak hours)
/// </summary>
[AutomaticRetry(Attempts = 3)]
public class CodeExpirationJob
{
    private readonly CodeExpirationService _codeExpirationService;
    private readonly ILogger<CodeExpirationJob> _logger;
    private readonly IClock _clock;
    public CodeExpirationJob(
        CodeExpirationService codeExpirationService,
        ILogger<CodeExpirationJob> logger,
        IClock clock)
    {
        _codeExpirationService = codeExpirationService;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>
    /// Execute the code expiration job
    /// </summary>
    [JobDisplayName("Code Expiration Job")]
    [DisableConcurrentExecution(10)]
    [Queue("default")]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("CodeExpirationJob started");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var expiredCount = await _codeExpirationService.ExpireCodesWithPastDateAsync();

            stopwatch.Stop();
            var finishedAt = _clock.Now;

            _logger.LogInformation(
                "CodeExpirationJob finished successfully. " +
                "Expired {ExpiredCount} code(s) in {ElapsedMs} ms at {FinishedAt}",
                expiredCount,
                stopwatch.ElapsedMilliseconds,
                finishedAt);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "CodeExpirationJob failed after {ElapsedMs} ms",
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
