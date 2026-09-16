using Droniverse.Community.Domain.Enums;
using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.DTO.Response.Mongo;

public record OrderOverviewDto(
    int TotalOrders,
    int PendingOrders,
    int SuccessOrders,
    int FailedOrders,
    int CancelledOrders,
    decimal TotalAmount
)
{
    public OrderOverviewDto() : this(0, 0, 0, 0, 0, 0m)
    {
    }
}