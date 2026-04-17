using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Request;
using MongoDB.Driver;

namespace Droniverse.Community.Domain.IRepository.Mongo;

public interface IOrderRepository
{
    //Payment
    Task<Payment> GetPaymentByOrderID(Guid orderID);
    Task<Payment?> UpdatePaymentStatus(Guid orderID, PaymentStatus status, Guid transactionId);
    Task<Payment?> AddPayment(Guid orderID, Payment payment);
    Task<Payment?> UpdatePayment(Guid orderID, Payment payment);
    Task<IEnumerable<Payment>> GetPaymentsByCondition(FilterDefinition<Order> filter);

    // Order
    Task<Order> GetOrderByTransactionID(Guid transactionId);
    Task<Order?> GetOrderByPaymentLinkId(string paymentLinkId);
    Task<Order?> GetOrderByOrderCode(long orderCode);
    Task<PaginationResult<IEnumerable<Order>>> GetOrders(OrderSearchRequest searchRequest);
    Task<PaginationResult<IEnumerable<Order>>> GetOrdersByConditionWithPagination(FilterDefinition<Order> filter, int currentPage, int pageSize);
    Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter);
    Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter);
    Task<Order?> AddOrder(Order order);
    Task<Order?> UpdateOrder(Order order);
    Task<bool?> DeleteOrder(Guid orderID);

    Task<IEnumerable<OrderRevenueData>> GetSuccessfulRevenueDataByClubId(
        Guid clubId,
        OrderType orderType = OrderType.USER_PURCHASE,
        DateTime? fromInclusive = null,
        DateTime? toExclusive = null);

    Task<RevenueOverviewOrderAggregateData> GetRevenueOverviewOrderAggregateByClubId(
        Guid clubId,
        DateTime startLastMonth,
        DateTime startThisMonth,
        DateTime startNextMonth);

    Task<IEnumerable<Order>> GetAllSuccessfulOrders();
}

