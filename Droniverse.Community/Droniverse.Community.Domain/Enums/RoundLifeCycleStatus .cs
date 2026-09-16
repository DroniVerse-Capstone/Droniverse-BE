using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Enums
{
    public enum RoundLifeCycleStatus : byte
    {
        Upcoming = 0,   // chưa tới giờ
        Ongoing = 1,    // đang diễn ra
        Finished = 2    // đã kết thúc
    }
}
