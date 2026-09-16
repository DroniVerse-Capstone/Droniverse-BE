using Droniverse.Identity.Domain.Enums;
using System;

namespace Droniverse.Identity.Application.DTO.Request;

public record SysPolicyCreateDto(
    Droniverse.Identity.Domain.Enums.SysPolicyType Type,
    string TitleEN,
    string TitleVN,
    string ContentEN,
    string ContentVN,
    DateTime EffectiveDate
);
