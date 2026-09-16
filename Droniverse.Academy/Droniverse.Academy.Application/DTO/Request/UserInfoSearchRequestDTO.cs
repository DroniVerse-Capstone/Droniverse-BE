using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;

namespace Droniverse.Academy.Application.DTO.Request
{
    public class UserInfoSearchRequestDTO 
    {
        public string? SearchName { get; set; }
        public SortDirection? SortDirection { get; set; } = Shared.Enums.SortDirection.Asc;
    }
}
