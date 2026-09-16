using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public class CompetitionCertificateAdditionResponse
    {
        public Guid CompetitionID { get; set; }
        public int AddedTotal { get; set; }
        public required List<SimpleCertificateResponse> Certificates { get; set; }
    }
}
