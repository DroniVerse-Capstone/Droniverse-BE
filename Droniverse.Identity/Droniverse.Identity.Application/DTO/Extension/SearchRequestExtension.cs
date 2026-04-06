using Droniverse.Shared.Enums;

namespace Droniverse.Identity.Application.DTO.Extension
{
    public class UserInfoSearchRequest
    {
        public string? SearchName { get; set; }
        public SortDirection? SortDirection { get; set; } = Shared.Enums.SortDirection.Asc;
    }

    public class UserSearchRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public SortDirection? SortDirection { get; set; }
    }
}
