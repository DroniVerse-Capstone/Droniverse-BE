using Droniverse.Identity.Domain.Enums;

namespace Droniverse.Identity.Domain.Interfaces;

public interface ISysPolicySearchSpecification
{
    SysPolicyType? Type { get; }
    string? Title { get; }
}
