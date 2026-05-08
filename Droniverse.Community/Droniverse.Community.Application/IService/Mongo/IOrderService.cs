using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Shared.DTOs.Request;
using MongoDB.Driver;

namespace Droniverse.Community.Application.IService.Mongo;
public interface IOrderService
{
    Task<OrderOverviewDto> GetOrdersOverview();
    Task<AllOrdersWithOverviewDto> GetAllOrdersWithOverview(OrderSearchRequest searchRequest);
    Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetAllOrders(OrderSearchRequest searchRequest);
    Task<List<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter);
    Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByConditionWithPagination(FilterDefinition<Order> filter, int currentPage, int pageSize);
    Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter);
    Task<OrderResponseDto?> AddOrder(Guid clubId, OrderCreateDto orderAddRequest);
    Task<OrderResponseDto?> UpdateOrder(OrderUpdateDto orderUpdateRequest);
    Task<bool> DeleteOrder(Guid orderID);

    Task<OrderResponseDto?> GetOrderByOrderId(Guid orderID);
    Task<IEnumerable<OrderResponseDto?>> GetOrdersByClubId(Guid clubId);
    Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByClubIdWithPagination(Guid clubId, int currentPage, int pageSize);
    Task<IEnumerable<OrderResponseDto?>> GetOrdersByCurrentClub();
    Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByCurrentClubWithPagination(int currentPage, int pageSize);
    Task<IEnumerable<OrderResponseDto?>> GetOrdersByCurrentUser();
    Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByCurrentUserWithPagination(int currentPage, int pageSize);
    Task<bool> CancelOrder(Guid orderId);
    Task<bool> ReceiveOrder(Guid orderId);

    /// <summary>
    /// Lấy danh sách chi tiết đơn hàng của một user theo userId.
    /// Dùng cho dashboard admin chi tiết.
    /// </summary>
    Task<IEnumerable<UserOrderDetailResponseDto>> GetOrdersDetailByUserId(Guid userId);

}

