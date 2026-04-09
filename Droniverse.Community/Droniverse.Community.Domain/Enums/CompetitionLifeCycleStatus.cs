using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Enums
{
    public enum CompetitionLifeCycleStatus
    {
        UPCOMING = 0,             // chưa tới thời gian hiển thị
        COMING_SOON = 1,          // đã visible nhưng chưa mở đăng ký
        REGISTRATION_OPEN = 2,    // đang mở đăng ký
        REGISTRATION_CLOSED = 3,  // đã đóng đăng ký, chờ thi
        ONGOING = 4,              // đang thi
        FINISHED = 5              // đã kết thúc (chưa publish result)
    }
}
