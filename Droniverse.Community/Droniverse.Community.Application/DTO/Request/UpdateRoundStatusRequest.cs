using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Request
{
    public class UpdateRoundStatusRequest
    {
        public UpdateRoundStatusEnum roundStatus { get; set; }
    }

    public enum UpdateRoundStatusEnum
    {
        Cancelled = 2
    }
}
