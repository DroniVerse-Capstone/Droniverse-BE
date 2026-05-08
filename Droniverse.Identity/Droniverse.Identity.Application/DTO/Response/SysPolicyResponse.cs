using System;
using Droniverse.Identity.Domain.Enums;

namespace Droniverse.Identity.Application.DTO.Response;

public record SysPolicyResponse(
    Guid SysPolicyID,
    SysPolicyType Type,
    string Title,
    string Content,
    DateTime EffectiveDate,
    DateTime CreatedAt,
    Guid CreatedBy
);
