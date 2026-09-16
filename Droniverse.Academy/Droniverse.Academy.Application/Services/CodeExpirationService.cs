using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Logging;

namespace Droniverse.Academy.Application.Services;

/// <summary>
/// Service for handling code expiration based on expireDate
/// </summary>
public class CodeExpirationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<CodeExpirationService> _logger;

    public CodeExpirationService(
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<CodeExpirationService> logger)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Marks codes as EXPIRED if their expireDate has passed
    /// </summary>
    public async Task<int> ExpireCodesWithPastDateAsync()
    {
        try
        {
            var now = _clock.Now;
            _logger.LogInformation("CodeExpirationService: Starting code expiration check at {Now}", now);

            // Get all AVAILABLE codes that have expired
            // Using GetAllAsync with filter and large pageSize to fetch all matching codes
            var result = await _unitOfWork.Codes.GetAllAsync(
                filter: c => c.Status == CodeStatus.AVAILABLE && c.ExpireDate < now,
                pageIndex: 1,
                pageSize: 10000  // Large batch size for daily run
            );

            var expiredCodeList = result.Data.ToList();
            int expiredCount = 0;

            if (expiredCodeList.Count == 0)
            {
                _logger.LogInformation("CodeExpirationService: No codes to expire at {Now}", now);
                return 0;
            }

            _logger.LogInformation("CodeExpirationService: Found {Count} codes to expire", expiredCodeList.Count);

            // Mark each code as expired using the entity method
            foreach (var code in expiredCodeList)
            {
                code.Expire(now);
                expiredCount++;
            }

            // Save changes in a transaction
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _unitOfWork.SaveChangesAsync();
            });

            _logger.LogInformation(
                "CodeExpirationService: Successfully expired {Count} codes at {Now}",
                expiredCount,
                now);

            return expiredCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CodeExpirationService: Error during code expiration process");
            throw;
        }
    }
}
