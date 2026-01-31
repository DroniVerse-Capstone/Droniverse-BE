using Droniverse.Community.Domain.Entities.Mongo;
using MongoDB.Driver;

namespace Droniverse.Community.Domain.IRepository.Mongo;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetOrders();
    Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter);
    Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter);
    Task<Order?> AddOrder(Order order);
    Task<Order?> UpdateOrder(Order order);
    Task<bool?> DeleteOrder(Guid orderID);
}

