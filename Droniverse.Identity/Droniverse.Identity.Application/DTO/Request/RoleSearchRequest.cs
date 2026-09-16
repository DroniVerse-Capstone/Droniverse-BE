using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.Application.DTO.Request;

public class RoleSearchRequest : SearchRequest, IRoleSearchSpecification
{
    public string? RoleName { get; set; }
    
    [FromQuery(Name = "sortDirection")]
    public SortDirection? SortDirection { get; set; }
}
