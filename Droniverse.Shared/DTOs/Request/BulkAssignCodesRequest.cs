namespace Droniverse.Shared.DTOs.Request;

public class BulkAssignCodesRequest
{
    public List<BulkAssignCodeItemRequest> Items { get; set; } = [];
    public bool SendEmail { get; set; }
}

public class BulkAssignCodeItemRequest
{
    public string CodeId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
