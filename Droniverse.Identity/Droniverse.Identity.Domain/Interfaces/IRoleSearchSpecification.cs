using Droniverse.Shared.Enums;

namespace Droniverse.Identity.Domain.Interfaces;

public interface IRoleSearchSpecification
{
    string? RoleName { get; }
    SortDirection? SortDirection { get; }
}
