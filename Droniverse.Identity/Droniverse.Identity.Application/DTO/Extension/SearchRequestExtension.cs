using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;

namespace Droniverse.Identity.Application.DTO.Extension
{
    public class UserInfoSearchRequest : SearchRequest
    {
        public string? SearchName { get; set; }
        public SortDirection? SortDirection { get; set; } = Shared.Enums.SortDirection.Asc;
    }
}
