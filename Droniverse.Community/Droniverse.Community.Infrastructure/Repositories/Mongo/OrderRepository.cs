using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.Exceptions;
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

    public async Task<Payment> GetPaymentByOrderID(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp._id, orderID);
        IAsyncCursor<Order> asyncCursor = _orders.FindAsync(filter).Result;
        Order? order = asyncCursor.FirstOrDefault();
        if (order is null)
        {
            throw new NotFoundException($"Payment with order id {orderID} not found");
        }
        Payment? payment = order?.Payment;
        if (payment is null)
        {
            throw new NotFoundException($"Payment with order id {orderID} not found");
        }

        return payment;

    }


    public async Task<IEnumerable<Payment>> GetPaymentsByCondition(FilterDefinition<Order> filter)
    {
        IAsyncCursor<Order> orderList = await _orders.FindAsync(filter);
        return orderList.ToList().Where(order => order.Payment != null).Select(order => order.Payment!);
    }

    public async Task<Payment?> UpdatePayment(Guid orderID, Payment payment)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Eq(temp => temp._id, orderID),
            Builders<Order>.Filter.Ne(temp => temp.Payment, null),
            Builders<Order>.Filter.Eq(temp => temp.Payment.TransactionID, payment.TransactionID)
        );
        var update = Builders<Order>.Update
            .Set(temp => temp.Payment, payment);
        var options = new FindOneAndUpdateOptions<Order>
        {
            ReturnDocument = ReturnDocument.After
        };
        var updatedOrder = await _orders.FindOneAndUpdateAsync(filter, update, options);
        if (updatedOrder?.Payment == null)
            throw new NotFoundException($"Payment with order id {orderID} and transaction id {payment.TransactionID} not found");
        return updatedOrder.Payment;
    }

    public async Task<Payment?> UpdatePaymentStatus(Guid orderID, PaymentStatus status, Guid transactionId)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Eq(temp => temp._id, orderID),
            Builders<Order>.Filter.Ne(temp => temp.Payment, null),
            Builders<Order>.Filter.Eq(temp => temp.Payment.TransactionID, transactionId)
        );

        var update = Builders<Order>.Update
            .Set(temp => temp.Payment.PaymentStatus, status);

        var options = new FindOneAndUpdateOptions<Order>
        {
            ReturnDocument = ReturnDocument.After
        };

        var updatedOrder = await _orders.FindOneAndUpdateAsync(filter, update, options);
        if (updatedOrder?.Payment == null)
            throw new NotFoundException($"Payment with order id {orderID} and transaction id {transactionId} not found");

        return updatedOrder.Payment;
    }

    public async Task<Payment?> AddPayment(Guid orderID, Payment payment)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp._id, orderID);
        UpdateDefinition<Order>? update = Builders<Order>.Update.Set(temp => temp.Payment, payment);
        FindOneAndUpdateOptions<Order>? options = new FindOneAndUpdateOptions<Order>
        {
            ReturnDocument = ReturnDocument.After
        };
        var updatedOrder = await _orders.FindOneAndUpdateAsync(filter, update, options);
        if (updatedOrder == null)
            throw new NotFoundException($"Order with id {orderID} not found");

        return updatedOrder.Payment;
    }

    public async Task<Order> GetOrderByTransactionID(Guid transactionId)
    {

        if (string.IsNullOrWhiteSpace(transactionId.ToString()))
            throw new NotFoundException($"Not found transaction id: {transactionId}");

        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Ne(x => x.Payment, null),
            Builders<Order>.Filter.Eq(x => x.Payment.TransactionID, transactionId)
        );

        var order = await _orders.Find(filter).FirstOrDefaultAsync();

        if (order == null)
            throw new NotFoundException($"Order with transaction id {transactionId} not found");

        return order;
    }

    public async Task<IEnumerable<OrderRevenueData>> GetSuccessfulRevenueDataByProductIds(
        IEnumerable<Guid> productIds,
        DateTime? fromInclusive = null,
        DateTime? toExclusive = null)
    {
        var ids = productIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
            return [];

        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.In(o => o.Item.ProductID, ids),
            Builders<Order>.Filter.Ne(o => o.Payment, null),
            Builders<Order>.Filter.Eq(o => o.Payment.PaymentStatus, PaymentStatus.SUCCESS));

        if (fromInclusive.HasValue)
            filter &= Builders<Order>.Filter.Gte(o => o.Payment.TransactionDate, fromInclusive.Value);

        if (toExclusive.HasValue)
            filter &= Builders<Order>.Filter.Lt(o => o.Payment.TransactionDate, toExclusive.Value);

        return await _orders
            .Find(filter)
            .Project(o => new OrderRevenueData
            {
                ProductId = o.Item.ProductID,
                Revenue = o.TotalAmount,
                PaidAt = o.Payment.TransactionDate
            })
            .ToListAsync();
    }
}

