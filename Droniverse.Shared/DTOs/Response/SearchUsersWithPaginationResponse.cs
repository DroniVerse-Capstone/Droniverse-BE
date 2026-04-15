using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs.Response
{
    public record SearchUsersWithPaginationResponse
    {
        public int TotalItems { get; set; }
        public required List<SimpleUserReponse> Items { get; set; }
    }
}
