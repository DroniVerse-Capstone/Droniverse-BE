using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs.Request
{
    public class SearchUsersWithPaginationRequest : SearchRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public List<Guid>? UserIds { get; set; }
    }
}
