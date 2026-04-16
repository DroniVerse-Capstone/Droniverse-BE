using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<OrderService> _logger;
    private readonly IEmailService _emailService;
    private readonly AcademyMicroserviceClient _academyMicroserviceClient;
    public OrderService(
        IOrderRepository orderRepository,
        IMapper mapper,
        IInvoiceRepository invoiceRepository,
        IPaymentService paymentService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<OrderService> logger,
        IEmailService emailService,
        AcademyMicroserviceClient academyMicroserviceClient)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _invoiceRepository = invoiceRepository;
        _paymentService = paymentService;
        _emailService = emailService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _academyMicroserviceClient = academyMicroserviceClient;
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
            throw new KeyNotFoundException("Không tìm thấy câu lạc bộ.");

        var userRoles = _currentUserService.Roles;
        var isMember = userRoles.Contains(Roles.ClubMember);
        //var isClubManager = userRoles.Contains(Roles.ClubManager);

        // lấy product info
        Product? product = await _unitOfWork.Products.GetByCondition(p => p.ProductID == orderAddRequest.Item.ProductID && p.Status == ProductStatus.ACTIVE,
            query => query.AsNoTracking());

        if (product == null)
            throw new KeyNotFoundException("Không tìm thấy thông tin của sản phẩm");

        //------------------------------------------------------------------------------------------------------------------------------
        
        // Validation for club member
        if (isMember)
        {
            if (orderAddRequest.Item.Quantity != 1 && orderAddRequest.Item.Type.Equals(ProductType.COURSE))
            {
                throw new Exception("Thành viên câu lạc bộ chỉ được mua 1 mã code cho sản phẩm mỗi đơn hàng.");
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------
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
            Total = isMember ? CalculateTotal(product.Price, quantity) * 1.1m : CalculateTotal(product.Price, quantity),
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
            TotalAmount = isMember ? CalculateTotal(product.Price, quantity) * 1.1m : CalculateTotal(product.Price, quantity),
            Payment = null // Chưa có payment khi tạo order
        };

        //Add order into db
        Order? createdOrder = await _orderRepository.AddOrder(order) ?? throw new Exception("Create order failed");
        try
        {
            // Gọi PayOS để tạo payment link
            var paymentCreateDto = new PaymentCreateDto
            {
                TotalAmount = order.TotalAmount,
                PaymentMethod = orderAddRequest.PaymentMethod
            };
            await _paymentService.CreatePaymentLink(createdOrder._id, paymentCreateDto);

            // FIX: Reload order from database to get the Payment data that was just added
            // CreatePaymentLink() calls _orderRepository.AddPayment() to save Payment to MongoDB
            // But the createdOrder object in memory is not automatically updated
            FilterDefinition<Order>? reloadFilter = Builders<Order>.Filter.Eq(o => o._id, createdOrder._id);
            createdOrder = await _orderRepository.GetOrderByCondition(reloadFilter) ?? createdOrder;

        }
        catch
        {
            // Đánh dấu order failed nếu tạo payment thất bại
            createdOrder.Status = OrderStatus.FAILED;
            await _orderRepository.UpdateOrder(createdOrder);
            throw;
        }

        //Gửi email thông báo đến đặt hàng thành công
        string? userName = _currentUserService.UserName;
        string? email = _currentUserService.Email;
        Guid productId = createdOrder.Item.ProductID;
        string? proNameVN = createdOrder.Item.ProductNameVN;
        string? proNameEN = createdOrder.Item.ProductNameEN;
        string? type = createdOrder.Item.Type.ToString();
        decimal unitOfPrice = createdOrder.Item.UnitOfPrice;
        //createdOrder.CreateAt
        //có quantity ở trên
        //createdOrder.TotalAmount

        await _emailService.SendOrderConfirmationEmailAsync(email!, userName!, createdOrder._id.ToString()!, createdOrder.CreateAt.ToString(), productId.ToString(), proNameVN, proNameEN, type, unitOfPrice, quantity, createdOrder.TotalAmount);
        //---------------------------------------------------------------------------------------------------------------

        return _mapper.Map<OrderResponseDto?>(createdOrder);
    }

    public Task<bool> DeleteOrder(Guid orderID)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<OrderResponseDto?>> GetOrdersByClubId(Guid clubId)
    {
        FilterDefinition<Order>? filter = Builders<Order>.Filter.Eq(o => o.ClubID, clubId);
        IEnumerable<Order?> orders = await _orderRepository.GetOrdersByCondition(filter);
        return _mapper.Map<IEnumerable<Order>, IEnumerable<OrderResponseDto?>>(orders);
    }

    public async Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByClubIdWithPagination(Guid clubId, int currentPage, int pageSize)
    {
        FilterDefinition<Order>? filter = Builders<Order>.Filter.Eq(o => o.ClubID, clubId);
        PaginationResult<IEnumerable<Order>> orders = await _orderRepository.GetOrdersByConditionWithPagination(filter, currentPage, pageSize);
        return _mapper.Map<PaginationResult<IEnumerable<Order>>, PaginationResult<IEnumerable<OrderResponseDto?>>>(orders);
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

    public async Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetAllOrders(OrderSearchRequest searchRequest)
    {
        PaginationResult<IEnumerable<Order>> orders = await _orderRepository.GetOrders(searchRequest);
        PaginationResult<IEnumerable<OrderResponseDto?>> orderDtos = _mapper.Map<PaginationResult<IEnumerable<Order>>, PaginationResult<IEnumerable<OrderResponseDto?>>>(orders);
        return orderDtos;

    }

    public async Task<List<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _orderRepository.GetOrdersByCondition(filter);
        return _mapper.Map<IEnumerable<Order?>, IEnumerable<OrderResponseDto?>>(orders).ToList();
    }

    public async Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByConditionWithPagination(FilterDefinition<Order> filter, int currentPage, int pageSize)
    {
        PaginationResult<IEnumerable<Order>> orders = await _orderRepository.GetOrdersByConditionWithPagination(filter, currentPage, pageSize);
        return _mapper.Map<PaginationResult<IEnumerable<Order>>, PaginationResult<IEnumerable<OrderResponseDto?>>>(orders);
    }

    public Task<OrderResponseDto?> UpdateOrder(OrderUpdateDto orderUpdateRequest)
    {
        throw new NotImplementedException();
    }

    private decimal CalculateTotal(decimal price, int quantity)
    {
        return price * quantity;
    }

    public async Task<IEnumerable<OrderResponseDto?>> GetOrdersByCurrentClub()
    {
        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("Người dùng chưa xác thực!");

        var isClubManager = _currentUserService.Roles.Contains(Roles.ClubManager);
        if (isClubManager)
        {
            //Lấy club theo approverId (Id của club manager)
            Club? club = await _unitOfWork.Participations.GetClubByApproverId(userId); //đã bắt Exception trong ParticipationRepo
            IEnumerable<OrderResponseDto?> orders = await GetOrdersByClubId(club.ClubID);
            return orders;
        }
        else
        {
            throw new UnauthorizedAccessException("Chỉ quản lý câu lạc bộ (Club Manager) mới có quyền truy cập đơn hàng của câu lạc bộ!");
        }
    }

    public async Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByCurrentClubWithPagination(int currentPage, int pageSize)
    {
        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("Người dùng chưa xác thực!");

        var isClubManager = _currentUserService.Roles.Contains(Roles.ClubManager);
        if (isClubManager)
        {
            //Lấy club theo approverId (Id của club manager)
            Club? club = await _unitOfWork.Participations.GetClubByApproverId(userId); //đã bắt Exception trong ParticipationRepo
            return await GetOrdersByClubIdWithPagination(club.ClubID, currentPage, pageSize);
        }
        else
        {
            throw new UnauthorizedAccessException("Chỉ quản lý câu lạc bộ (Club Manager) mới có quyền truy cập đơn hàng của câu lạc bộ!");
        }
    }

    public async Task<IEnumerable<OrderResponseDto?>> GetOrdersByCurrentUser()
    {
        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("Người dùng chưa xác thực!");

        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(o => o.UserID, userId);
        IEnumerable<Order?> orders = await _orderRepository.GetOrdersByCondition(filter);
        IEnumerable<OrderResponseDto?> response = _mapper.Map<IEnumerable<OrderResponseDto?>>(orders);
        return response;
    }

    public async Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByCurrentUserWithPagination(int currentPage, int pageSize)
    {
        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("Người dùng chưa xác thực!");

        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(o => o.UserID, userId);
        return await GetOrdersByConditionWithPagination(filter, currentPage, pageSize);
    }

    public async Task<bool> CancelOrder(Guid orderId)
    {
        try
        {
            Order? order = await _orderRepository.GetOrderByCondition(Builders<Order>.Filter.Eq(o => o._id, orderId));
            if (order == null)
                throw new NotFoundException($"Không tìm thấy đơn hàng với mã đơn hàng #{orderId}");
            order.Status = OrderStatus.CANCELLED;
            await _orderRepository.UpdateOrder(order);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi HỦY đơn hàng với mã đơn hàng #{orderId}");
            return false;
        }
    }

    public async Task<bool> ReceiveOrder(Guid orderId)
    {
        try
        {
            Order? order = await _orderRepository.GetOrderByCondition(Builders<Order>.Filter.Eq(o => o._id, orderId));
            if (order == null)
                throw new NotFoundException($"Không tìm thấy đơn hàng với mã đơn hàng #{orderId}");
            order.Status = OrderStatus.RECEIVED;
            await _orderRepository.UpdateOrder(order);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi NHẬN đơn hàng với mã đơn hàng #{orderId}");
            return false;
        }
    }
}

