using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;

namespace Droniverse.Community.Application.DTO.Request;

public class WithdrawSearchRequest : SearchRequest
{
    public WithdrawStatus? Status { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
}