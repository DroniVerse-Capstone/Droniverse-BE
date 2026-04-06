using Droniverse.Shared.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs
{
    public record PagedCourseBulkResponse
    {
        public int TotalItems { get; set; }
        public required IEnumerable<CourseBulkResponseDTO> Items { get; set; }

    }
}
