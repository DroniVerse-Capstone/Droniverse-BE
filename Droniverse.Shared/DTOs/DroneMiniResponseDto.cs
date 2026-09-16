using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs
{
    public class DroneMiniResponseDto
    {
        public Guid DroneID { get; set; }
        public string DroneNameVN { get; set; } = string.Empty;
        public string DroneNameEN { get; set; } = string.Empty;
    }
}
