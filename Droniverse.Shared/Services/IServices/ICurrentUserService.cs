using System.Security.Claims;

namespace Droniverse.Shared.Services.IServices;

public interface ICurrentUserService
{
    string? UserID { get; }
    Guid UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    ClaimsPrincipal User { get; }
}
