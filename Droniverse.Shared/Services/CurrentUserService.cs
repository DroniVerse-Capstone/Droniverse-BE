using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Droniverse.Shared.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserID => _httpContextAccessor.HttpContext?.User
        .FindFirst("UserID")?.Value;

    public Guid UserId
    {
        get
        {
            if (!IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            if (string.IsNullOrWhiteSpace(UserID))
                throw new UnauthorizedAccessException("UserID claim not found");

            if (!Guid.TryParse(UserID, out var userId))
                throw new UnauthorizedAccessException("Invalid UserID claim format");

            return userId;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.Name)?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.Email)?.Value;

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User
        .FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
}
