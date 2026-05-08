using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace Droniverse.Community.Application.DTO.Request;

public class ClubCreationRequestCreateDto
{
    [Required]
    public Guid DroneID { get; set; }

    [Required]
    [AllowHtml]
    public string ClubPolicyVN { get; set; }

    [Required]
    [AllowHtml]
    public string ClubPolicyEN { get; set; }

    [AllowHtml]
    public string? ClubRequirement { get; set; }

    [Required]
    public Guid Media { get; set; }

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

    [Range(1, 100000)]
    public int LimitParticipant { get; set; }

    [Required]
    public Guid Image { get; set; }
}

