using Droniverse.Identity.Domain.Enums;
using System;

namespace Droniverse.Identity.Application.DTO.Request;

public record SysPolicyCreateDto(
    Droniverse.Identity.Domain.Enums.SysPolicyType Type,
    string Title,
    string Content,
    DateTime EffectiveDate
);
