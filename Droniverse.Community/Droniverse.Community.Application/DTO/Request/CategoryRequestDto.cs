using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Request
{
    public record CategoryRequestDto
    (
        [Required]
        [Length(1,255, ErrorMessage = "Category name must be from 1 to 255 characters !!")]
        string TypeNameVN,
        [Required]
        [Length(1,255, ErrorMessage = "Category name must be from 1 to 255 characters !!")]
        string TypeNameEN,
        string DescriptionVN,
        string DescriptionEN
    );
}
