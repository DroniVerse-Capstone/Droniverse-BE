using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    /// <summary>
    /// Response cho Club overview - chứa doanh thu và KPI giao dịch
    /// </summary>
    public record RevenueOverviewResponse
    {
        // ===== REVENUE =====
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }

        // ===== KPI =====
        public int TotalTransactions { get; set; }
        public int TransactionsThisMonth { get; set; }
    }

    /// <summary>
    /// Response cho Admin overview - chứa Doanh thu, Lợi nhuận và KPI giao dịch (không có Chi phí)
    /// </summary>
    public record AdminRevenueOverviewResponse
    {
        // ===== REVENUE =====
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public double RevenueGrowthRate { get; set; }

        // ===== PROFIT =====
        public decimal NetProfit { get; set; }
        public decimal ProfitThisMonth { get; set; }
        public decimal ProfitLastMonth { get; set; }
        public double ProfitGrowthRate { get; set; }

        // ===== KPI =====
        public int TotalTransactions { get; set; }
        public int TransactionsThisMonth { get; set; }

        // ===== EXPANDED KPI =====
        public double SuccessRate { get; set; }

        public decimal RevenueToday { get; set; }
        public decimal RevenueYesterday { get; set; }

        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueLastWeek { get; set; }

        public decimal RevenueThisYear { get; set; }
        public decimal RevenueLastYear { get; set; }

        public int TransactionsLastMonth { get; set; }
    }
}

