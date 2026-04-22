using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Response
{
    public class DroneMiniResponse
    {
        public Guid DroneID { get; set; }
        public string Name { get; set; }
        public string ImgURL { get; set; } 

    }

}
