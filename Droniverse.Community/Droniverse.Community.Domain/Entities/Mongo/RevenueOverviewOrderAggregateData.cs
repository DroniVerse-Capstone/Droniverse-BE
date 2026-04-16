namespace Droniverse.Community.Domain.Entities.Mongo;

public class RevenueOverviewOrderAggregateData
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }

    public decimal TotalExpense { get; set; }
    public decimal ExpenseThisMonth { get; set; }
    public decimal ExpenseLastMonth { get; set; }

    public int TotalTransactions { get; set; }
    public int TransactionsThisMonth { get; set; }
}
