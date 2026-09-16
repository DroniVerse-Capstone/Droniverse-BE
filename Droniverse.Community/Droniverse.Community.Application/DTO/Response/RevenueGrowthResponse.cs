using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record RevenueGrowthResponse
    {
        public required List<MonthlyStat> RevenueGrowth { get; set; }
        public decimal TotalValue { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class MonthlyStat
    {
        public DateTime Month { get; set; }   // safer than string
        public decimal Value { get; set; }  // số liệu (member / revenue / ...)
    }
}
