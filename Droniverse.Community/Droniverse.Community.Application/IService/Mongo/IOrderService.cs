using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Shared.DTOs.Request;
using MongoDB.Driver;

namespace Droniverse.Community.Application.IService.Mongo;
public interface IOrderService
{
    Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetAllOrders(OrderSearchRequest searchRequest);
    Task<List<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter);
    Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter);
    Task<OrderResponseDto?> AddOrder(Guid clubId, OrderCreateDto orderAddRequest);
    Task<OrderResponseDto?> UpdateOrder(OrderUpdateDto orderUpdateRequest);
    Task<bool> DeleteOrder(Guid orderID);

    Task<OrderResponseDto?> GetOrderByOrderId(Guid orderID);
    Task<IEnumerable<OrderResponseDto?>> GetOrdersByClubId(Guid clubId);
    Task<IEnumerable<OrderResponseDto?>> GetOrdersByCurrentClub();
    Task<IEnumerable<OrderResponseDto?>> GetOrdersByCurrentUser();

}

