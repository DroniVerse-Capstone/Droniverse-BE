using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Enums
{
    public enum CompetitionStatus : byte
    {
        DRAFT = 0,
        OPEN = 1,
        CLOSED = 2,
        ONGOING = 3,
        FINISHED = 4,
        CANCELLED = 5
    }
}
