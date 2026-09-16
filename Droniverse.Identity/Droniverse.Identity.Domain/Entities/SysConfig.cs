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
        public required string Email { get; set; }
        public required string FacebookUrl { get; set; }
        public required string PhoneNumber { get; set; }
        public required string LogoSystem { get; set; }
        public int BufferEstimatedDuration { get; set; }
        public required string LogoCertificate { get; set; }
        public required string CertificateTemplateUrl { get; set; }
    }   
}
