using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record RevenueOverviewResponse
    {
        // ===== REVENUE =====
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public double RevenueGrowthRate { get; set; }

        // ===== EXPENSE =====
        public decimal TotalExpense { get; set; }
        public decimal ExpenseThisMonth { get; set; }
        public decimal ExpenseLastMonth { get; set; }

        // ===== PROFIT =====
        public decimal NetProfit { get; set; }
        public decimal ProfitThisMonth { get; set; }
        public decimal ProfitLastMonth { get; set; }
        public double ProfitGrowthRate { get; set; }

        // ===== KPI =====
        public int TotalTransactions { get; set; }
        public int TransactionsThisMonth { get; set; }
    }
}
