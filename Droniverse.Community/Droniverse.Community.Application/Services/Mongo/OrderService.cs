using AutoMapper;
using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using MongoDB.Driver;

namespace Droniverse.Community.Application.Services.Mongo;

internal class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMapper _mapper;
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUserService;
    public OrderService(
        IOrderRepository orderRepository, 
        IMapper mapper, 
        IInvoiceRepository invoiceRepository,
        IPaymentService paymentService,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _invoiceRepository = invoiceRepository;
        _paymentService = paymentService;
        _currentUserService = currentUserService;
    }

    public async Task<OrderResponseDto?> AddOrder(OrderCreateDto orderAddRequest)
    {
        if (orderAddRequest == null)
            throw new ArgumentNullException(nameof(orderAddRequest));

        if(orderAddRequest.Items is null || orderAddRequest.Items.Count == 0)
            throw new ArgumentNullException("Đơn hàng phải chứa ít nhất 1 item.", nameof(orderAddRequest.Items));

        if(!Guid.TryParse(_currentUserService.UserID, out var currentUserId))
            throw new UnauthorizedAccessException("Nguời dùng chưa được xác thực");

        if(orderAddRequest.TotalAmount <= 0)
            throw new ArgumentException("Tổng tiền phải lớn hơn 0.", nameof(orderAddRequest.TotalAmount));

        // Chặn user giả mạo userId từ req
        if(orderAddRequest.UserID != Guid.Empty && orderAddRequest.UserID != currentUserId)
            throw new UnauthorizedAccessException("UserId trong request không hợp lệ.");

        // Tạo order items
        List<OrderItem> orderItems = orderAddRequest.Items.Select(item => new OrderItem
        {
            ProductID = item.ProductID,
            ProductName = item.ProductName,
            Type = item.Type,
            UnitOfPrice = item.UnitOfPrice,
            Quantity = item.Quantity,
            Total = item.Total
        }).ToList();

        Order order = new Order()
        {
            _id = Guid.NewGuid(),
            UserID = currentUserId,
            InvoiceID = Guid.NewGuid(),
            TotalAmount = orderAddRequest.TotalAmount,
            Status = OrderStatus.PENDING,
            CreateAt = DateTime.UtcNow.AddHours(7),
            Items = orderItems
        };

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
        //Tạo order
        Order? createdOrder = await _orderRepository.AddOrder(order) ?? throw new Exception("Create order failed");

        try
        {
            PaymentCreateDto paymentReq = new PaymentCreateDto(
            TotalAmount: order.TotalAmount,
            PaymentMethod: orderAddRequest.PaymentMethod
        );

            await _paymentService.CreatePaymentLink(createdOrder._id, paymentReq);
        }
        catch
        {
            // Đánh dấu order failed nếu tạo payment thất bại
            createdOrder.Status = OrderStatus.FAILED;
            await _orderRepository.UpdateOrder(createdOrder);
            throw;
        }

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
        Invoice? responseInvoice = await _invoiceRepository.AddInvoice(invoice);
        return _mapper.Map<OrderResponseDto?>(createdOrder);
    }

    public Task<bool> DeleteOrder(Guid orderID)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        var order = await _orderRepository.GetOrderByCondition(filter);
        return _mapper.Map<Order, OrderResponseDto?>(order);   
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

    public async Task<List<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _orderRepository.GetOrdersByCondition(filter);
        return _mapper.Map<IEnumerable<Order?>, IEnumerable<OrderResponseDto?>>(orders).ToList();
    }

    public Task<OrderResponseDto?> UpdateOrder(OrderUpdateDto orderUpdateRequest)
    {
        throw new NotImplementedException();
    }
}

