using AutoMapper;
using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository.Mongo;
using MongoDB.Driver;

namespace Droniverse.Community.Application.Services.Mongo;

internal class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMapper _mapper;
    public OrderService(IOrderRepository orderRepository, IMapper mapper, IInvoiceRepository invoiceRepository)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<OrderResponseDto?> AddOrder(OrderCreateDto orderAddRequest)
    {
        if (orderAddRequest == null)
            return null;
        Order order = new Order();
        order._id = Guid.NewGuid();
        order.UserID = Guid.NewGuid();
        order.InvoiceID = Guid.NewGuid();
        order.Payment = new Payment
        {
            TransactionID = "test",
            PaymentMethod = PaymentMethod.VNPAY,
            PaymentStatus = PaymentStatus.SUCCESS,
            TransactionDate = DateTime.UtcNow
        };
        order.TotalAmount = orderAddRequest.TotalAmount;
        order.Status = OrderStatus.PENDING;
        order.CreateAt = DateTime.UtcNow;
        order.Items = new List<OrderItem>();
        foreach (var item in orderAddRequest.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductID = item.ProductID,
                ProductName = item.ProductName,
                Type = item.Type,
                UnitOfPrice = item.UnitOfPrice,
                Quantity = item.Quantity,
                Total = item.Total
            });
        }
        Order? createdOrder = await _orderRepository.AddOrder(order);

        //Add Invoice
        Invoice invoice= new Invoice();
        invoice._id = Guid.NewGuid();
        invoice.TotalAmount = order.TotalAmount;
        invoice.ContentEN = "Content invoice...";
        invoice.ContentVN = "Nội dung hóa đơn...";
        invoice.IssueAt = DateTime.UtcNow;
        invoice.CustomerInfo = new CustomerInfo
        {
            UserID = order.UserID,
            Name = "Tuyền đẹp trai",
            TaxCode = "xxx-yyy-zzz"
        };
        invoice.OrderID = createdOrder!._id;
        Invoice? responseInvoice = await _invoiceRepository.AddInvoice(invoice);
        return _mapper.Map<OrderResponseDto?>(createdOrder);
    }

    public Task<bool> DeleteOrder(Guid orderID)
    {
        throw new NotImplementedException();
    }

    public Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        throw new NotImplementedException();
    }

    public async Task<List<OrderResponseDto?>> GetOrders()
    {
        IEnumerable<Order> orders = await _orderRepository.GetOrders();
        IEnumerable<OrderResponseDto?> orderDtos = _mapper.Map<IEnumerable<Order>, IEnumerable<OrderResponseDto?>>(orders);
        foreach (var orderDto in orderDtos)
        {
            var order = orders.FirstOrDefault(o => o._id == orderDto.OrderID);
            if (order != null)
            {
                var itemDtos = _mapper.Map<List<OrderItem>, List<OrderItemDto>>(order.Items);
                orderDto.Items.AddRange(itemDtos);
            }
        }
        return orderDtos.ToList();

    }

    public Task<List<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        throw new NotImplementedException();
    }

    public Task<OrderResponseDto?> UpdateOrder(OrderUpdateDto orderUpdateRequest)
    {
        throw new NotImplementedException();
    }
}

