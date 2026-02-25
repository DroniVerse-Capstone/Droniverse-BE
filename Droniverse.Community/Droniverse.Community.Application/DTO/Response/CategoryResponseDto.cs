using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record CategoryResponseDto(
        Guid CategoryId,
        string TypeNameVN,
        string TypeNameEN,
        string DescriptionVN,
        string DescriptionEN
    );
}
