using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using AutoMapper;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.Services
{
    public class DetailDashboardService : IDetailDashboardService
    {
        private readonly AcademyMicroserviceClient _academyClient;
        private readonly IOrderService _orderService;
        private readonly ITransactionService _transactionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly IMapper _mapper;

        public DetailDashboardService(
            AcademyMicroserviceClient academyClient,
            IOrderService orderService,
            ITransactionService transactionService,
            IUnitOfWork unitOfWork,
            IOrderRepository orderRepository,
            IdentityMicroserviceClient identityMicroserviceClient,
            IMapper mapper)
        {
            _academyClient = academyClient;
            _orderService = orderService;
            _transactionService = transactionService;
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _identityMicroserviceClient = identityMicroserviceClient;
            _mapper = mapper;
        }

        public async Task<PaginationResult<IEnumerable<DetailDashboardUserResponse>>> GetClubMembersWithCourseSpend(int page = 1, int pageSize = 10)
        {
            if (page < 1)
                page = 1;

            if (pageSize <= 0)
                pageSize = 10;

            var users = await _identityMicroserviceClient.GetListUserByRole("CLUB_MEMBER");
            if (users == null || !users.Any())
                return new PaginationResult<IEnumerable<DetailDashboardUserResponse>>(Enumerable.Empty<DetailDashboardUserResponse>(), 0, page, pageSize);

            var courseSpendByUserId = await BuildCourseSpendByUserIdAsync();

            var detailUsers = _mapper.Map<List<DetailDashboardUserResponse>>(users);
            foreach (var user in detailUsers)
            {
                user.TotalSpent = courseSpendByUserId.TryGetValue(user.UserId, out var totalSpent)
                    ? totalSpent
                    : 0m;
            }

            var orderedUsers = detailUsers
                .OrderByDescending(user => user.TotalSpent)
                .ThenBy(user => user.FullName)
                .ToList();

            var totalRecords = orderedUsers.Count;
            var pagedUsers = orderedUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginationResult<IEnumerable<DetailDashboardUserResponse>>(pagedUsers, totalRecords, page, pageSize);
        }

        public async Task<PaginationResult<IEnumerable<DetailDashboardClubManagerResponse>>> GetClubManagersWithWalletBalance(int page = 1, int pageSize = 10)
        {
            if (page < 1)
                page = 1;

            if (pageSize <= 0)
                pageSize = 10;

            var users = await _identityMicroserviceClient.GetListUserByRole("CLUB_MANAGER");
            if (users == null || !users.Any())
                return new PaginationResult<IEnumerable<DetailDashboardClubManagerResponse>>([], 0, page, pageSize);

            var userIds = users.Select(u => u.UserId).ToList();

            var wallets = await _unitOfWork.Wallets.GetManyByCondition(w => userIds.Contains(w.OwnerID));
            var walletDict = wallets.ToDictionary(w => w.OwnerID, w => w.Balance);

            var detailUsers = users.Select(u => new DetailDashboardClubManagerResponse
            {
                UserId = u.UserId,
                FullName = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                AvatarUrl = u.AvatarUrl,
                WalletBalance = walletDict.TryGetValue(u.UserId, out var balance) ? balance : 0m
            })
            .OrderByDescending(u => u.WalletBalance)
            .ToList();

            var totalRecords = detailUsers.Count;
            var pagedUsers = detailUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginationResult<IEnumerable<DetailDashboardClubManagerResponse>>(pagedUsers, totalRecords, page, pageSize);
        }

        private async Task<Dictionary<Guid, decimal>> BuildCourseSpendByUserIdAsync()
        {
            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null || !allOrders.Any())
                return [];

            return allOrders
                .Where(order => order.OrderType == OrderType.USER_PURCHASE
                    && order.Item != null
                    && order.Item.Type == ProductType.COURSE)
                .GroupBy(order => order.UserID)
                .ToDictionary(group => group.Key, group => group.Sum(order => order.TotalAmount));
        }

        public async Task<IEnumerable<UserOrderDetailResponseDto>> GetUserOrderDetails(Guid userId)
        {
            return await _orderService.GetOrdersDetailByUserId(userId);
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetUserTransactions(Guid userId)
        {
            return await _transactionService.GetTransactionsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<CourseStatisticInterServiceDto>> GetCourseDashboard()
        {
            return await _academyClient.GetCourseDashboard();
        }

        public async Task<IEnumerable<CourseDetailDashboardResponseDto>> GetCourseRevenueDashboard()
        {
            var courses = await _academyClient.GetCourseDashboard();
            var courseIds = courses.Select(c => c.CourseId).ToList();
            
            // Lấy danh sách Product map với các CourseId (ReferenceID = CourseId)
            var products = await _unitOfWork.Products.GetManyByCondition(p => courseIds.Contains(p.ReferenceID));
            var productIdToCourseId = products.ToDictionary(p => p.ProductID, p => p.ReferenceID);

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            var courseRevenues = allOrders != null 
                ? allOrders
                    .Where(o => o.Item != null && productIdToCourseId.ContainsKey(o.Item.ProductID))
                    .GroupBy(o => productIdToCourseId[o.Item!.ProductID]) // Group by CourseId (ReferenceID)
                    .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount))
                : [];

            var result = courses.Select(c => new CourseDetailDashboardResponseDto
            {
                CourseId = c.CourseId,
                CurrentVersionId = c.CurrentVersionId,
                TitleVN = c.TitleVN,
                TitleEN = c.TitleEN,
                ImageUrl = c.ImageUrl,
                TotalLearners = c.TotalLearners,
                AverageRating = c.AverageRating,
                TotalRevenue = courseRevenues.TryGetValue(c.CourseId, out var rev) ? rev : 0m
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToList();

            return result;
        }
    }
}
