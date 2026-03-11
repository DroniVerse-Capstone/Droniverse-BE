using System.Security.Claims;

namespace Droniverse.Shared.Services;

public interface ICurrentUserService
{
    string? UserID { get; }
    string? UserName { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    ClaimsPrincipal User { get; }
}
