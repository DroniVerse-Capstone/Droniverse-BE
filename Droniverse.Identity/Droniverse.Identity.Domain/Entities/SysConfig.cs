using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Domain.Entities
{
    public class SysConfig
    {
        public Guid SysConfigID { get; set; }
        public string Email { get; set; }
        public string FacebookUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string logoSystem { get; set; }
        public string logoCertificate { get; set; }
    }
}
