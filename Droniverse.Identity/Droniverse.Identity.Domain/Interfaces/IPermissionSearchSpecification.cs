using Droniverse.Shared.Enums;

namespace Droniverse.Identity.Domain.Interfaces;

public interface IPermissionSearchSpecification
{
    string? PermissionName { get; }
    SortDirection? SortDirection { get; }
}
