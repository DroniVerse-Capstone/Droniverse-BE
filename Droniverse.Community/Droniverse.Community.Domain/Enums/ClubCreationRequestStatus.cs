using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Enums
{
    public enum ClubCreationRequestStatus : byte
    {
        PENDING = 0,
        APPROVED = 1,
        REJECTED = 2,
        CANCEL = 3
    }
}
