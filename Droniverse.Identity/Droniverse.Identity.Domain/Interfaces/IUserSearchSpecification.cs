using Droniverse.Shared.Enums;

namespace Droniverse.Identity.Domain.Interfaces;

public interface IUserSearchSpecification
{
    string? Username { get; }
    string? Email { get; }
    RoleNameEnum? RoleName { get; }
    SortDirection? SortDirection { get; }
}

