using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public class CompetitionCertificateDeletionResponse
    {
        public Guid CompetitionID { get; set; }
        public int DeletedTotal { get; set; }
        public required List<SimpleCertificateResponse> RemainingCertificates { get; set; }
    }
}
