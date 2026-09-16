using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs
{
    public class SimpleCertificateResponse
    {
        public Guid CertificateID { get; set; }
        public required string CertificateNameVN { get; set; }
        public required string CertificateNameEN { get; set; }
        public required string ImageUrl { get; set; }
    }
}
