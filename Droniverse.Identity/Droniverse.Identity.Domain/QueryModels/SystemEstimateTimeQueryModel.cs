using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Domain.QueryModels
{
    public record SystemEstimateTimeQueryModel
    {
        public int BufferEstimatedDuration { get; init; }
    }
}
