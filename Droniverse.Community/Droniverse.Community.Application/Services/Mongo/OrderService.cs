using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request.Mongo;
using Droniverse.Community.Application.DTO.Response;
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
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Messages.Notification;
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
    private readonly IOrderNotificationPublisher _orderNotificationPublisher;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly IClock _clock;
    public OrderService(
        IOrderRepository orderRepository,
        IMapper mapper,
        IInvoiceRepository invoiceRepository,
        IPaymentService paymentService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<OrderService> logger,
        IEmailService emailService,
        AcademyMicroserviceClient academyMicroserviceClient,
        IOrderNotificationPublisher orderNotificationPublisher,
        IdentityMicroserviceClient identityMicroserviceClient,
        IClock clock)
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
        _orderNotificationPublisher = orderNotificationPublisher;
        _identityMicroserviceClient = identityMicroserviceClient;
        _clock = clock;
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
            Total = CalculateTotal(product.Price, quantity),
        };

        // Tạo order
        Order order = new Order
        {
            _id = Guid.NewGuid(),
            UserID = currentUserId,
            UserEmail = _currentUserService.Email ?? string.Empty,
            UserName = _currentUserService.UserName ?? string.Empty,
            ClubID = clubId,
            CreateAt = _clock.Now,
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

            // Publish notification event after order is successfully created
            try
            {
                var userEmail = createdOrder?.UserEmail;
                var userName = createdOrder?.UserName;

                if (createdOrder != null && !string.IsNullOrWhiteSpace(userEmail))
                {
                    var notificationEvent = new OrderCreatedNotificationMessage(
                        UserId: createdOrder.UserID,
                        OrderId: createdOrder._id,
                        UserEmail: userEmail,
                        UserName: userName ?? "User",
                        Total: createdOrder.TotalAmount,
                        CreatedAt: _clock.Now
                    );

                    await _orderNotificationPublisher.PublishOrderCreatedAsync(notificationEvent);
                    _logger.LogInformation($"Order created notification published for order {createdOrder._id}");
                }
                else
                {
                    _logger.LogWarning($"Skipped publishing order.created notification - createdOrder: {createdOrder != null}, userEmail: {userEmail}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish order created notification");
                // Don't throw - order creation should succeed even if notification fails
            }
        }
        catch
        {
            // Đánh dấu order failed nếu tạo payment thất bại
            createdOrder.Status = OrderStatus.FAILED;
            await _orderRepository.UpdateOrder(createdOrder);
            throw;
        }

        var orderDto = _mapper.Map<OrderResponseDto?>(createdOrder);
        return orderDto;
    }

    public Task<bool> DeleteOrder(Guid orderID)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<OrderResponseDto?>> GetOrdersByClubId(Guid clubId)
    {
        FilterDefinition<Order>? filter = Builders<Order>.Filter.Eq(o => o.ClubID, clubId);
        IEnumerable<Order?> orders = await _orderRepository.GetOrdersByCondition(filter);
        return await BuildOrderResponseDtosWithUsersAsync(orders);
    }

    public async Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByClubIdWithPagination(Guid clubId, int currentPage, int pageSize)
    {
        FilterDefinition<Order>? filter = Builders<Order>.Filter.Eq(o => o.ClubID, clubId);
        PaginationResult<IEnumerable<Order>> orders = await _orderRepository.GetOrdersByConditionWithPagination(filter, currentPage, pageSize);
        return await BuildOrderResponsePaginationWithUsersAsync(orders);
    }

    public async Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        var order = await _orderRepository.GetOrderByCondition(filter);
        if (order == null)
            return null;

        return await BuildOrderResponseDtoWithUserAsync(order);
    }

    public async Task<OrderResponseDto?> GetOrderByOrderId(Guid orderID)
    {
        FilterDefinition<Order>? filter = Builders<Order>.Filter.Eq(o => o._id, orderID);
        Order order = await _orderRepository.GetOrderByCondition(filter) ?? throw new NotFoundException($"Không tìm thấy đơn hàng với mã đơn hàng #{orderID}");
        return await BuildOrderResponseDtoWithUserAsync(order);
    }

    public async Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetAllOrders(OrderSearchRequest searchRequest)
    {
        PaginationResult<IEnumerable<Order>> orders = await _orderRepository.GetOrders(searchRequest);
        return await BuildOrderResponsePaginationWithUsersAsync(orders);
    }

    public async Task<List<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _orderRepository.GetOrdersByCondition(filter);
        return (await BuildOrderResponseDtosWithUsersAsync(orders)).ToList();
    }

    public async Task<PaginationResult<IEnumerable<OrderResponseDto?>>> GetOrdersByConditionWithPagination(FilterDefinition<Order> filter, int currentPage, int pageSize)
    {
        PaginationResult<IEnumerable<Order>> orders = await _orderRepository.GetOrdersByConditionWithPagination(filter, currentPage, pageSize);
        return await BuildOrderResponsePaginationWithUsersAsync(orders);
    }

    public Task<OrderResponseDto?> UpdateOrder(OrderUpdateDto orderUpdateRequest)
    {
        throw new NotImplementedException();
    }

    private async Task<IEnumerable<OrderResponseDto?>> BuildOrderResponseDtosWithUsersAsync(IEnumerable<Order?> orders)
    {
        var orderList = orders.Where(order => order != null).Cast<Order>().ToList();
        if (orderList.Count == 0)
            return [];

        var userMap = await GetUsersByIdsMapAsync(orderList.Select(order => order.UserID));
        var clubMap = await GetClubsByIdsMapAsync(orderList.Select(order => order.ClubID));

        return orderList.Select(order =>
        {
            var orderDto = _mapper.Map<OrderResponseDto?>(order);
            if (orderDto == null)
                return null;

            return orderDto with 
            { 
                User = GetUserForOrder(order, userMap),
                Club = GetClubForOrder(order, clubMap)
            };
        });
    }

    private async Task<OrderResponseDto?> BuildOrderResponseDtoWithUserAsync(Order order)
    {
        var orderDto = _mapper.Map<OrderResponseDto?>(order);
        if (orderDto == null)
            return null;

        var user = await GetUserByIdOrFallbackAsync(order);
        var club = await GetClubByIdOrFallbackAsync(order);
        
        return orderDto with 
        { 
            User = user,
            Club = club
        };
    }

    private async Task<PaginationResult<IEnumerable<OrderResponseDto?>>> BuildOrderResponsePaginationWithUsersAsync(PaginationResult<IEnumerable<Order>> orders)
    {
        var orderList = orders.Data?.Where(order => order != null).Cast<Order>().ToList() ?? [];
        if (orderList.Count == 0)
            return new PaginationResult<IEnumerable<OrderResponseDto?>>(Enumerable.Empty<OrderResponseDto?>(), orders.TotalRecords, orders.PageIndex, orders.PageSize);

        var userMap = await GetUsersByIdsMapAsync(orderList.Select(order => order.UserID));
        var clubMap = await GetClubsByIdsMapAsync(orderList.Select(order => order.ClubID));

        var responseOrders = orderList.Select(order =>
        {
            var orderDto = _mapper.Map<OrderResponseDto?>(order);
            if (orderDto == null)
                return null;

            return orderDto with 
            { 
                User = GetUserForOrder(order, userMap),
                Club = GetClubForOrder(order, clubMap)
            };
        }).ToList();

        return new PaginationResult<IEnumerable<OrderResponseDto?>>(responseOrders, orders.TotalRecords, orders.PageIndex, orders.PageSize);
    }

    private async Task<Dictionary<Guid, UserResponse>> GetUsersByIdsMapAsync(IEnumerable<Guid> userIds)
    {
        var ids = userIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return [];

        try
        {
            var users = await _identityMicroserviceClient.GetUsersBulk(ids);
            return users.ToDictionary(user => user.UserId, user => user);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load users for order response enrichment.");
            return [];
        }
    }

    private async Task<UserResponse> GetUserByIdOrFallbackAsync(Order order)
    {
        var user = await _identityMicroserviceClient.GetUserByUserID(order.UserID);
        if (user != null)
            return user;

        return new UserResponse(
            order.UserID,
            order.UserName,
            string.Empty,
            string.Empty,
            order.UserEmail,
            null,
            string.Empty,
            null,
            default,
            null,
            [],
            []);
    }

    private UserResponse GetUserForOrder(Order order, IReadOnlyDictionary<Guid, UserResponse> userMap)
    {
        if (userMap.TryGetValue(order.UserID, out var user))
            return user;

        return new UserResponse(
            order.UserID,
            order.UserName,
            string.Empty,
            string.Empty,
            order.UserEmail,
            null,
            string.Empty,
            null,
            default,
            null,
            [],
            []);
    }

    private async Task<Dictionary<Guid, ClubMiniResponse>> GetClubsByIdsMapAsync(IEnumerable<Guid> clubIds)
    {
        var ids = clubIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return [];

        try
        {
            var clubs = await _unitOfWork.Clubs.GetManyByConditionAsQueryable(
                c => ids.Contains(c.ClubID),
                q => q.AsNoTracking())
                .ToListAsync();

            return clubs.ToDictionary(
                club => club.ClubID,
                club => new ClubMiniResponse
                {
                    ClubID = club.ClubID,
                    NameVN = club.NameVN,
                    NameEN = club.NameEN,
                    ImageUrl = club.ImageUrl ?? string.Empty
                });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load clubs for order response enrichment.");
            return [];
        }
    }

    private async Task<ClubMiniResponse?> GetClubByIdOrFallbackAsync(Order order)
    {
        if (order.ClubID == Guid.Empty)
            return null;

        try
        {
            var club = await _unitOfWork.Clubs.GetByCondition(
                c => c.ClubID == order.ClubID,
                q => q.AsNoTracking());

            if (club != null)
            {
                return new ClubMiniResponse
                {
                    ClubID = club.ClubID,
                    NameVN = club.NameVN,
                    NameEN = club.NameEN,
                    ImageUrl = club.ImageUrl ?? string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load club for order response enrichment.");
        }

        return null;
    }

    private ClubMiniResponse? GetClubForOrder(Order order, IReadOnlyDictionary<Guid, ClubMiniResponse> clubMap)
    {
        if (order.ClubID == Guid.Empty)
            return null;

        if (clubMap.TryGetValue(order.ClubID, out var club))
            return club;

        return null;
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

    public async Task<OrderOverviewDto> GetOrdersOverview()
    {
        var allOrders = await _orderRepository.GetOrdersByCondition(Builders<Order>.Filter.Empty);
        var orderList = allOrders.Where(order => order != null).Cast<Order>().ToList();

        int totalOrders = orderList.Count;
        int pendingOrders = orderList.Count(o => o.Status == OrderStatus.PENDING);
        int successOrders = orderList.Count(o => o.Status == OrderStatus.SUCCESS);
        int failedOrders = orderList.Count(o => o.Status == OrderStatus.FAILED);
        int cancelledOrders = orderList.Count(o => o.Status == OrderStatus.CANCELLED);
        decimal totalAmount = orderList.Sum(o => o.TotalAmount);

        return new OrderOverviewDto(
            TotalOrders: totalOrders,
            PendingOrders: pendingOrders,
            SuccessOrders: successOrders,
            FailedOrders: failedOrders,
            CancelledOrders: cancelledOrders,
            TotalAmount: totalAmount
        );
    }

    public async Task<AllOrdersWithOverviewDto> GetAllOrdersWithOverview(OrderSearchRequest searchRequest)
    {
        var overview = await GetOrdersOverview();
        var orders = await GetAllOrders(searchRequest);

        return new AllOrdersWithOverviewDto(overview, orders);
    }


    public async Task<IEnumerable<OrderResponseDto?>> GetOrdersByCurrentUser()
    {
        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("Người dùng chưa xác thực!");

        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(o => o.UserID, userId);
        IEnumerable<Order?> orders = await _orderRepository.GetOrdersByCondition(filter);
        var response = _mapper.Map<IEnumerable<OrderResponseDto?>>(orders);
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
            order.Status = OrderStatus.SUCCESS;
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


