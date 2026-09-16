using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;

namespace Droniverse.Community.Application.DTO.Request;

/// <summary>
/// DTO cho việc search/filter Transaction
/// </summary>
public class TransactionSearchRequest : SearchRequest
{
    public TransactionType? Type { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
}
