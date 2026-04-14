using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Domain.Entities.Mongo;
using MongoDB.Driver;

namespace Droniverse.Community.Application.IService.Mongo;
public interface IOrderService
{
    Task<List<OrderResponseDto?>> GetOrders();
    Task<List<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter);
    Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter);
    Task<OrderResponseDto?> AddOrder(Guid clubId, OrderCreateDto orderAddRequest);
    Task<OrderResponseDto?> UpdateOrder(OrderUpdateDto orderUpdateRequest);
    Task<bool> DeleteOrder(Guid orderID);

    Task<OrderResponseDto?> GetOrderByOrderId(Guid orderID);
    Task<IEnumerable<OrderResponseDto?>> GetOrderByClubId(Guid clubId);
}

