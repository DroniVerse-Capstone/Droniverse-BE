using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.Application.DTO.Extension
{
    public class UserInfoSearchRequest
    {
        public string? SearchName { get; set; }
        public SortDirection? SortDirection { get; set; } = Shared.Enums.SortDirection.Asc;
    }

    public class UserSearchRequest : SearchRequest, IUserSearchSpecification
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        [FromQuery(Name = "roleName")]
        public RoleNameEnum? RoleName { get; set; } 
        public SortDirection? SortDirection { get; set; }
    }
}
