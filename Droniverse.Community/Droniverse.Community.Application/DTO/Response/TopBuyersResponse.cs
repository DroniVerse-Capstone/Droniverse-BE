namespace Droniverse.Community.Application.DTO.Response;

/// <summary>
/// Thông tin chi tiết 1 buyer trong bảng xếp hạng top buyers.
/// </summary>
public class BuyerStatItem
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal TotalSpent { get; set; }
    public int PurchaseCount { get; set; }
}

/// <summary>
/// Response chứa danh sách top buyers — dùng cho cả Admin (toàn hệ thống) và Club Manager (theo club).
/// </summary>
public class TopBuyersResponse
{
    public List<BuyerStatItem> Buyers { get; set; } = [];
    public decimal TotalSystemRevenue { get; set; }
}
