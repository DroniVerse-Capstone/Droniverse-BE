using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.IRepository.Mongo;
using MongoDB.Driver;

namespace Droniverse.Community.Infrastructure.Repositories.Mongo;

internal class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;
    private readonly string collectionName = "orders";
    public OrderRepository(IMongoDatabase mongoDatabase)
    {
        _orders = mongoDatabase.GetCollection<Order>(collectionName);
    }

    public async Task<IEnumerable<Order>> GetOrders()
    {
        IAsyncCursor<Order> orderList = await _orders.FindAsync(Builders<Order>.Filter.Empty);
        return orderList.ToList();
    }
    public async Task<Order> AddOrder(Order order)
    {
        //order._id = Guid.NewGuid();
        foreach(OrderItem orderItem in order.Items)
        {
            orderItem.ProductID = Guid.NewGuid();
        }

        await _orders.InsertOneAsync(order);
        return order;
    }

    public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IAsyncCursor<Order> orderList = await _orders.FindAsync(filter);
        return orderList.ToList();
    }

    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        IAsyncCursor<Order> order = await _orders.FindAsync(filter);
        return order.FirstOrDefault();
    }

    public async Task<Order?> UpdateOrder(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp._id, order._id);
        Order? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();
        if (existingOrder == null)
        {
            return null;
        }

        order._id = existingOrder._id;

        ReplaceOneResult replaceOneResult = await _orders.ReplaceOneAsync(filter, order);
        return replaceOneResult.ModifiedCount > 0 ? order : null;
    }

    public async Task<bool?> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp._id, orderID);
        Order? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();
        if (existingOrder == null)
            return false;
        DeleteResult deleteResult = await _orders.DeleteOneAsync(filter);
        return deleteResult.DeletedCount > 0;
    }
}

