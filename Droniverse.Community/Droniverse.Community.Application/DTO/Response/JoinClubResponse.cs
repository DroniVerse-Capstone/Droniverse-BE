using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public class JoinClubResponse
    {
        public Guid ClubID { get; set; }
        public required string NameVN { get; set; }
        public required string NameEN { get; set; }
        public Guid? ClubAttemptRequestID { get; set; }
        public bool ClubIsPublic { get; set; }
    }   
}
