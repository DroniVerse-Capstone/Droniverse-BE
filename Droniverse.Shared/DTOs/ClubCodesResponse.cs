using Droniverse.Shared.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs
{
    public record ClubCodesResponse
    {
        public required string ClubID { get; set; }
        public int TotalItems { get; set; }
        public required List<CodeEntryResponse> CodesItem { get; set; }
    }

    public record CodeEntryResponse
    {
        public required string Code { get; set; }
        public required string CourseID { get; set; }
        public bool IsUsed { get; set; }
        public SimpleUserReponse? ComsumerInfo { get; set; }
        public DateTime ExpireDate { get; set; }
    }

}
