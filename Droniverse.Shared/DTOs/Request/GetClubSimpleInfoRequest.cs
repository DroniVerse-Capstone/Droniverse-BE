using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs.Request
{
    public record GetClubSimpleInfoRequest
    {
        public required List<Guid> ClubIds { get; set; }
    }
}
