using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record SimpleLabResponse
    {
        public Guid LabID { get; set; }
        public required string LabNameVN { get; set; }
        public required string LabNameEN { get; set; }
    }
}
