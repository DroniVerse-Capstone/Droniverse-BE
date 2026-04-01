using AutoMapper;
using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository.Mongo;
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

        if(orderAddRequest.Item is null)
            throw new ArgumentNullException("Đơn hàng không có sản phẩm.", nameof(orderAddRequest.Item));

        if(_currentUserService.UserID == null)
            throw new UnauthorizedAccessException("Không tìm thấy người dùng.");

        if(!Guid.TryParse(_currentUserService.UserID, out var currentUserId))
            throw new UnauthorizedAccessException("Nguời dùng chưa được xác thực.");

        if(orderAddRequest.TotalAmount <= 0)
            throw new ArgumentException("Tổng tiền phải lớn hơn 0.", nameof(orderAddRequest.TotalAmount));

        // Tạo order item (chỉ có 1 item cho mỗi order
        OrderItem orderItem = new OrderItem
        {
            ProductID = orderAddRequest.Item.ProductID,
            ProductName = orderAddRequest.Item.ProductName,
            Type = orderAddRequest.Item.Type,
            UnitOfPrice = orderAddRequest.Item.UnitOfPrice,
            Quantity = orderAddRequest.Item.Quantity,
            Total = orderAddRequest.Item.Total
        };

        //Tạo order
        Order order = new Order
        {
            _id = Guid.NewGuid(),
            UserID = currentUserId,
            CreateAt = DateTime.UtcNow.AddDays(7),
            InvoiceID = Guid.Empty, // Chưa có invoice khi tạo order
            Item = orderItem,
            Status = OrderStatus.PENDING,
            TotalAmount = orderAddRequest.TotalAmount,
            Payment = null // Chưa có payment khi tạo order
        };

        //Add order into db
        Order? createdOrder = await _orderRepository.AddOrder(order) ?? throw new Exception("Create order failed");
        try
        {
            // ===== MOCK PAYMENT - Skip PayOS for testing =====
            //var mockPayment = new Payment
            //{
            //    TransactionID = order._id,
            //    PaymentMethod = orderAddRequest.PaymentMethod,
            //    PaymentStatus = PaymentStatus.PENDING,
            //    TransactionDate = DateTime.UtcNow.AddHours(7),
            //    PaymentUrl = "http://localhost:5125/community/payment-success"
            //};
            //await _orderRepository.AddPayment(createdOrder._id, mockPayment);
            // ===== END MOCK =====

            // Gọi PayOS để tạo payment link
            var paymentCreateDto = new PaymentCreateDto
            {
                TotalAmount = order.TotalAmount,
                PaymentMethod = orderAddRequest.PaymentMethod
            };
            await _paymentService.CreatePaymentLink(createdOrder._id, paymentCreateDto);

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
        invoice.IssueAt = DateTime.UtcNow.AddDays(7);
        invoice.CustomerInfo = new CustomerInfo
        {
            UserID = order.UserID,
            Name = _currentUserService.UserName,
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

