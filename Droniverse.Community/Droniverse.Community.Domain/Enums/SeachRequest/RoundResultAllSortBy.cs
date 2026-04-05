using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Enums.SeachRequest
{
    public enum RoundResultAllSortBy
    {
        Point,          // Sắp xếp theo điểm
        ExecutionTime,  // Sắp xếp theo thời gian làm bài
        NumberOfSteps,  // Sắp xếp theo số bước thực hiện
        StartedAt,      // Sắp xếp theo thời gian bắt đầu
    }
}
