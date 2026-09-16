using Droniverse.Shared.DTOs;
using Droniverse.Identity.Domain.Enums;
using Droniverse.Identity.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.Application.DTO.Request;

public class SysPolicySearchRequest : SearchRequest, ISysPolicySearchSpecification
{
    public SysPolicyType? Type { get; set; }
    public string? Keyword { get; set; }
}
