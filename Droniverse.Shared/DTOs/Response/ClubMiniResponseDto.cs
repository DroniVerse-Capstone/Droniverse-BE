using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs.Response
{
    public class ClubMiniResponseDto
    {
        public Guid ClubID { get; set; }
        public string NameVN { get; set; } //varchar(255)
        public string NameEN { get; set; } //varchar(255)
        public string? ImageUrl { get; set; }
    }
}
