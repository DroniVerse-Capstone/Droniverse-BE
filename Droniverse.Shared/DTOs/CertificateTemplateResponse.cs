using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs
{
    public record CertificateTemplateResponse
    {
        public required string ImageUrl { get; set; }
    }
}
