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
                throw new UnauthorizedAccessException("Người dùng chưa được xác thực");

            if (string.IsNullOrWhiteSpace(UserID))
                throw new UnauthorizedAccessException("Không tìm thấy thông tin UserID trong token");

            if (!Guid.TryParse(UserID, out var userId))
                throw new UnauthorizedAccessException("Định dạng UserID không hợp lệ");

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
