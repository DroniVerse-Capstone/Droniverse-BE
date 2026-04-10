using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Response
{
    public record PagedManagerCoursesBulkResponse
    {
        public int TotalItems { get; set; }
        public required IEnumerable<ManagerCoursesBulkResponseDTO> Items { get; set; }
    }
}
