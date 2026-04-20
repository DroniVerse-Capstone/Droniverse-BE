using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Request
{
    public record ClubJoinDto(

        [Length(6, 6, ErrorMessage = "Club code must be 6 characters !")]
        string clubCode,

        Guid mediaID
        )
    { }
}
