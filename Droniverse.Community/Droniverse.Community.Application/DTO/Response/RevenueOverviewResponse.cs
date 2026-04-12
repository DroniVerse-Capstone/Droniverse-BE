using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record RevenueOverviewResponse
    {
        public decimal TotalRevenue { get; set; }        // tổng toàn bộ
        public decimal RevenueThisMonth { get; set; }    // tháng hiện tại
        public decimal RevenueLastMonth { get; set; }    // tháng trước
        public double GrowthRate { get; set; }           // % tăng trưởng GrowthRate = (thisMonthlastMonth) / lastMonth * 100
    }
}
