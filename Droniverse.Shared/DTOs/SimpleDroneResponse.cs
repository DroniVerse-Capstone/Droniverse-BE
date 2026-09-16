using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs
{
    public record SimpleDroneResponse
    {
        public Guid DroneId { get; set; }
        public required string DroneNameVN { get; set; }
        public required string DroneNameEN { get; set; }
    }
}
