using AutoMapper;
using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.Constants;
using Droniverse.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Droniverse.Community.Application.Services.Mongo;

internal class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMapper _mapper;
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    public OrderService(
        IOrderRepository orderRepository,
        IMapper mapper,
        IInvoiceRepository invoiceRepository,
        IPaymentService paymentService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _invoiceRepository = invoiceRepository;
        _paymentService = paymentService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto?> AddOrder(Guid clubId, OrderCreateDto orderAddRequest)
    {
        if (orderAddRequest == null)
            throw new ArgumentNullException(nameof(orderAddRequest));

        if (orderAddRequest.Item is null)
            throw new ArgumentNullException("Đơn hàng không có sản phẩm.", nameof(orderAddRequest.Item));

        if (_currentUserService.UserID == null)
            throw new UnauthorizedAccessException("Không tìm thấy người dùng.");

        if (!Guid.TryParse(_currentUserService.UserID, out var currentUserId))
            throw new UnauthorizedAccessException("Nguời dùng chưa được xác thực.");

        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId && c.Status == ClubStatus.ACTIVE, query => query.AsNoTracking());
        if (club == null)
            throw new KeyNotFoundException("Không tìm thấy câu lạc bộ");

        var userRoles = _currentUserService.Roles;
        var isMember = userRoles.Contains(Roles.ClubMember);
        //var isClubManager = userRoles.Contains(Roles.ClubManager);

        // lấy product info
        Product? product = await _unitOfWork.Products.GetByCondition(p => p.ProductID == orderAddRequest.Item.ProductID && p.Status == ProductStatus.ACTIVE,
            query => query.AsNoTracking());

        if (product == null)
            throw new KeyNotFoundException("Không tìm thấy thông tin của sản phẩm");

        int quantity = orderAddRequest.Item.Quantity;

        // Tạo order item (chỉ có 1 item cho mỗi order
        OrderItem orderItem = new OrderItem
        {
            ProductID = orderAddRequest.Item.ProductID,
            ProductNameVN = product.ProductNameVN,
            ProductNameEN = product.ProductNameEN,
            Type = orderAddRequest.Item.Type,
            UnitOfPrice = product.Price,
            Quantity = quantity,
            Total = CalculateTotal(product.Price, quantity),
        };

        // Tạo order
        Order order = new Order
        {
            _id = Guid.NewGuid(),
            UserID = currentUserId,
            ClubID = clubId,
            CreateAt = DateTime.UtcNow.AddHours(7),
            OrderType = isMember ? OrderType.USER_PURCHASE : OrderType.CLUB_IMPORT,
            Item = orderItem,
            Status = OrderStatus.PENDING,
            TotalAmount = CalculateTotal(product.Price, quantity),
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
        //Invoice invoice = new Invoice();
        //invoice._id = Guid.NewGuid();
        //invoice.TotalAmount = order.TotalAmount;
        //invoice.ContentVN =
        //        $"Thanh toán {order.Item.ProductName} " +
        //        $"(SL: {order.Item.Quantity}) " +
        //        $"với tổng tiền {order.TotalAmount:N0} VND";
        //invoice.ContentEN =
        //        $"Payment for {order.Item.ProductName} " +
        //        $"(Qty: {order.Item.Quantity}) " +
        //        $"with total amount {order.TotalAmount:N0} VND";
        //invoice.IssueAt = DateTime.UtcNow.AddHours(7);
        //invoice.CustomerInfo = new CustomerInfo
        //{
        //    UserID = order.UserID,
        //    FullName = _currentUserService.UserName!,
        //    Email = _currentUserService.Email!,
        //    TaxCode = null,
        //};
        //Invoice? responseInvoice = await _invoiceRepository.AddInvoice(invoice);

        return _mapper.Map<OrderResponseDto?>(createdOrder);
    }

    public Task<bool> DeleteOrder(Guid orderID)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderResponseDto?> GetOrderByClubId(Guid clubId)
    {
        FilterDefinition<Order>? filter = Builders<Order>.Filter.Eq(o => o.ClubID, clubId);
        IEnumerable<Order?> orders = await _orderRepository.GetOrdersByCondition(filter);
        return _mapper.Map<IEnumerable<Order>, IEnumerable<OrderResponseDto?>>(orders).FirstOrDefault();
    }

    public async Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        var order = await _orderRepository.GetOrderByCondition(filter);
        return _mapper.Map<Order, OrderResponseDto?>(order);
    }

    public async Task<OrderResponseDto?> GetOrderByOrderId(Guid orderID)
    {
        FilterDefinition<Order>? filter = Builders<Order>.Filter.Eq(o => o._id, orderID);
        Order order = await _orderRepository.GetOrderByCondition(filter) ?? throw new NotFoundException($"Không tìm thấy đơn hàng với mã đơn hàng #{orderID}");
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

    private decimal CalculateTotal(decimal price, int quantity)
    {
        return price * quantity;
    }
}

