using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public class ClubCreationRequestCreateResponseDto
    {
        public Guid ClubCreationRequestID { get;  set; }
        public string NameVN { get; set; }
        public string NameEN { get; set; }
    }
}
