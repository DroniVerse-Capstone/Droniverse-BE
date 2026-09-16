namespace Droniverse.Community.Application.DTO.Response.Mongo;

public record AllOrdersWithOverviewDto(
    OrderOverviewDto Overview,
    PaginationResult<IEnumerable<OrderResponseDto?>> Orders
)
{
}
