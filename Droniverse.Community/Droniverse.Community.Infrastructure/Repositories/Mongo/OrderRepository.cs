using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
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

    public async Task<PaginationResult<IEnumerable<Order>>> GetOrders(OrderSearchRequest searchRequest)
    {
        // Xây dựng bộ lọc từ các điều kiện tìm kiếm
        var filters = new List<FilterDefinition<Order>>();

        // Lọc theo ClubId
        if (searchRequest.ClubId.HasValue && searchRequest.ClubId != Guid.Empty)
        {
            filters.Add(Builders<Order>.Filter.Eq(o => o.ClubID, searchRequest.ClubId.Value));
        }

        // Lọc theo BuyerId
        if (searchRequest.BuyerId.HasValue && searchRequest.BuyerId != Guid.Empty)
        {
            filters.Add(Builders<Order>.Filter.Eq(o => o.UserID, searchRequest.BuyerId.Value));
        }

        // Lọc theo CreateAt (ngày tạo)
        if (searchRequest.CreateAt.HasValue)
        {
            var startDate = searchRequest.CreateAt.Value.Date;
            var endDate = startDate.AddDays(1);
            filters.Add(Builders<Order>.Filter.And(
                Builders<Order>.Filter.Gte(o => o.CreateAt, startDate),
                Builders<Order>.Filter.Lt(o => o.CreateAt, endDate)
            ));
        }

        // Lọc theo ReceiveDate
        if (searchRequest.ReceiveDate.HasValue)
        {
            var startDate = searchRequest.ReceiveDate.Value.Date;
            var endDate = startDate.AddDays(1);
            filters.Add(Builders<Order>.Filter.And(
                Builders<Order>.Filter.Gte(o => o.ReceivedAt, startDate),
                Builders<Order>.Filter.Lt(o => o.ReceivedAt, endDate)
            ));
        }

        // Lọc theo Status
        if (searchRequest.Status.HasValue)
        {
            var statusValue = (OrderStatus)(int)searchRequest.Status.Value;
            filters.Add(Builders<Order>.Filter.Eq(o => o.Status, statusValue));
        }

        // Lọc theo Type (nếu không phải giá trị default)
        if (searchRequest.Type != 0) // Giả sử 0 là giá trị mặc định không lọc
        {
            var typeValue = (OrderType)(int)searchRequest.Type;
            filters.Add(Builders<Order>.Filter.Eq(o => o.OrderType, typeValue));
        }

        // Kết hợp tất cả các filter với AND logic
        var combinedFilter = filters.Count > 0
            ? Builders<Order>.Filter.And(filters)
            : Builders<Order>.Filter.Empty;

        // Tính số record bỏ qua
        int skip = (searchRequest.CurrentPage - 1) * searchRequest.PageSize;

        // Lấy data với phân trang (skip & take)
        var orders = await _orders
            .Find(combinedFilter)
            .Skip(skip)
            .Limit(searchRequest.PageSize)
            .ToListAsync();

        // Tính tổng số record theo filter
        long totalRecords = await _orders.CountDocumentsAsync(combinedFilter);

        // Tạo kết quả phân trang
        PaginationResult<IEnumerable<Order>> result = new PaginationResult<IEnumerable<Order>>(
            orders,
            (int)totalRecords,
            searchRequest.CurrentPage,
            searchRequest.PageSize
        );
        return result;
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

    public async Task<PaginationResult<IEnumerable<Order>>> GetOrdersByConditionWithPagination(FilterDefinition<Order> filter, int currentPage, int pageSize)
    {
        int skip = (currentPage - 1) * pageSize;

        var orders = await _orders
            .Find(filter)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();

        long totalRecords = await _orders.CountDocumentsAsync(filter);

        return new PaginationResult<IEnumerable<Order>>(
            orders,
            (int)totalRecords,
            currentPage,
            pageSize
        );
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

    public async Task<Order?> GetOrderByPaymentLinkId(string paymentLinkId)
    {
        if (string.IsNullOrWhiteSpace(paymentLinkId))
            throw new ArgumentException($"PaymentLinkId cannot be null or empty");

        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Ne(x => x.Payment, null),
            Builders<Order>.Filter.Eq(x => x.Payment.PaymentLinkID, paymentLinkId)
        );

        var order = await _orders.Find(filter).FirstOrDefaultAsync();
        return order;
    }

    public async Task<Order?> GetOrderByOrderCode(long orderCode)
    {
        if (orderCode <= 0)
            throw new ArgumentException($"OrderCode must be greater than 0");

        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Ne(x => x.Payment, null),
            Builders<Order>.Filter.Eq(x => x.Payment.PaymentLinkID, orderCode.ToString())
        );

        var order = await _orders.Find(filter).FirstOrDefaultAsync();
        return order;
    }

    public async Task<RevenueOverviewOrderAggregateData> GetRevenueOverviewOrderAggregateByClubId(
     Guid clubId,
     DateTime startLastMonth,
     DateTime startThisMonth,
     DateTime startNextMonth)
    {
        if (clubId == Guid.Empty)
            return new RevenueOverviewOrderAggregateData();

        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Eq(o => o.ClubID, clubId),
            Builders<Order>.Filter.Eq(o => o.Status, OrderStatus.SUCCESS),
            Builders<Order>.Filter.Ne(o => o.Payment, null),
            Builders<Order>.Filter.Eq(o => o.Payment.PaymentStatus, PaymentStatus.SUCCESS)
        );

        var rawOrders = await _orders
                .Aggregate()
                .Match(filter)
                .ToListAsync();

        foreach (var o in rawOrders)
        {
            Console.WriteLine(o.TotalAmount.GetType());
            Console.WriteLine(
                "OrderId={0} | ClubId={1} | UserId={2} | Type={3} | Status={4} | Amount={5} | CreateAt={6} | " +
                "PaymentStatus={7} | PaymentMethod={8} | TransactionDate={9} | PaymentRef={10}",

                o._id,
                o.ClubID,
                o.UserID,
                o.OrderType,
                o.Status,
                o.TotalAmount,
                o.CreateAt,
                o.Payment?.PaymentStatus,
                o.Payment?.PaymentMethod,
                o.Payment?.TransactionDate,
                o.Payment?.Reference
            );
        }

        var aggregate = await _orders
            .Aggregate()
            .Match(filter)
            .Group(_ => 1, g => new RevenueOverviewOrderAggregateData
            {
                TotalRevenue = g.Sum(x =>
                    x.OrderType == OrderType.USER_PURCHASE
                        ? x.TotalAmount
                        : 0),

                RevenueThisMonth = g.Sum(x =>
                    x.OrderType == OrderType.USER_PURCHASE &&
                    x.Payment.TransactionDate >= startThisMonth &&
                    x.Payment.TransactionDate < startNextMonth
                        ? x.TotalAmount
                        : 0),

                RevenueLastMonth = g.Sum(x =>
                    x.OrderType == OrderType.USER_PURCHASE &&
                    x.Payment.TransactionDate >= startLastMonth &&
                    x.Payment.TransactionDate < startThisMonth
                        ? x.TotalAmount
                        : 0),

                TotalExpense = g.Sum(x =>
                    x.OrderType == OrderType.CLUB_IMPORT
                        ? x.TotalAmount
                        : 0),

                ExpenseThisMonth = g.Sum(x =>
                    x.OrderType == OrderType.CLUB_IMPORT &&
                    x.Payment.TransactionDate >= startThisMonth &&
                    x.Payment.TransactionDate < startNextMonth
                        ? x.TotalAmount
                        : 0),

                ExpenseLastMonth = g.Sum(x =>
                    x.OrderType == OrderType.CLUB_IMPORT &&
                    x.Payment.TransactionDate >= startLastMonth &&
                    x.Payment.TransactionDate < startThisMonth
                        ? x.TotalAmount
                        : 0),

                TotalTransactions = g.Sum(_ => 1),

                TransactionsThisMonth = g.Sum(x =>
                    x.Payment.TransactionDate >= startThisMonth &&
                    x.Payment.TransactionDate < startNextMonth
                        ? 1
                        : 0)
            })
            .FirstOrDefaultAsync();

        return aggregate ?? new RevenueOverviewOrderAggregateData();
    }

    public async Task<IEnumerable<OrderRevenueData>> GetSuccessfulRevenueDataByClubId(
        Guid clubId,
        OrderType orderType = OrderType.USER_PURCHASE,
        DateTime? fromInclusive = null,
        DateTime? toExclusive = null)
    {
        if (clubId == Guid.Empty)
            return [];

        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Eq(o => o.ClubID, clubId),
            Builders<Order>.Filter.Eq(o => o.Status, OrderStatus.SUCCESS),
            Builders<Order>.Filter.Ne(o => o.Payment, null),
            Builders<Order>.Filter.Eq(o => o.Payment.PaymentStatus, PaymentStatus.SUCCESS),
            Builders<Order>.Filter.Eq(o => o.OrderType, orderType)); // Filter by specified OrderType (USER_PURCHASE for revenue, CLUB_IMPORT for expenses)

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

    public async Task<IEnumerable<Order>> GetAllSuccessfulOrders()
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Eq(o => o.Status, OrderStatus.SUCCESS),
            Builders<Order>.Filter.Ne(o => o.Payment, null),
            Builders<Order>.Filter.Eq(o => o.Payment.PaymentStatus, PaymentStatus.SUCCESS));

        return await _orders
            .Find(filter)
            .ToListAsync();
    }

    // Debug method to get all orders regardless of status
    public async Task<IEnumerable<Order>> GetAllOrdersDebug()
    {
        return await _orders
            .Find(Builders<Order>.Filter.Empty)
            .ToListAsync();
    }
}


