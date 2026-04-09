using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs
{
    public record CategoryResponse
    (
        Guid CategoryId,
        string TypeNameVN,
        string TypeNameEN,
        string DescriptionVN,
        string DescriptionEN
    );
}
