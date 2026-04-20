using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Request
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;

    public class ClubCreationRequestCreateDto
    {
        [Required]
        public Guid DroneID { get; set; }

        [Required]
        public Guid ClubPolicyID { get; set; }

        [Required]
        [StringLength(255)]
        public string NameVN { get; set; }

        [Required]
        [StringLength(255)]
        public string NameEN { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        [Range(1, 100000)]
        public int LimitParticipant { get; set; }

        [Range(1, 20)]
        public int LimitClubManager { get; set; }

        [Required]
        public string Image { get; set; }
    }
}
