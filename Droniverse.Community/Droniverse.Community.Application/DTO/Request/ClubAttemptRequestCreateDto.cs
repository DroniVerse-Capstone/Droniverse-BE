using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Request
{
    public record ClubAttemptRequestCreateDto(
        [Required]
        Guid RequesterID,
        [Required]
        Guid ClubID
        )
    {
    }
}
