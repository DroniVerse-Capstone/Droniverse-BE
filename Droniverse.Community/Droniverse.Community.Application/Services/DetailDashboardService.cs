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

        public async Task<PaginationResult<IEnumerable<CourseDetailDashboardResponseDto>>> GetCourseRevenueDashboard(int page = 1, int pageSize = 10)
        {
            // Chỉ lấy khóa học đã PUBLISH
            var courses = (await _academyClient.GetCourseDashboard())
                .Where(c => c.IsPublished)
                .ToList();
            var courseIds = courses.Select(c => c.CourseId).ToList();
            
            var products = await _unitOfWork.Products.GetManyByCondition(p => courseIds.Contains(p.ReferenceID));
            var productIdToCourseId = products.ToDictionary(p => p.ProductID, p => p.ReferenceID);

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            var courseRevenues = allOrders != null 
                ? allOrders
                    .Where(o => o.Item != null && productIdToCourseId.ContainsKey(o.Item.ProductID))
                    .GroupBy(o => productIdToCourseId[o.Item!.ProductID])
                    .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount))
                : [];

            var result = courses.Select(c => new CourseDetailDashboardResponseDto
            {
                CourseId = c.CourseId,
                CurrentVersionId = c.CurrentVersionId,
                TitleVN = c.TitleVN,
                TitleEN = c.TitleEN,
                ImageUrl = c.ImageUrl,
                IsPublished = c.IsPublished,
                TotalLearners = c.TotalLearners,
                AverageRating = c.AverageRating,
                TotalRevenue = courseRevenues.TryGetValue(c.CourseId, out var rev) ? rev : 0m
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToList();

            var totalRecords = result.Count;
            var paged = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PaginationResult<IEnumerable<CourseDetailDashboardResponseDto>>(paged, totalRecords, page, pageSize);
        }

        public async Task<PaginationResult<IEnumerable<CourseRevenueByClubResponseDto>>> GetCourseRevenueByClub(Guid courseId, int page = 1, int pageSize = 10)
        {
            // Xác nhận khóa học có tồn tại và đã PUBLISH
            var courses = await _academyClient.GetCourseDashboard();
            if (!courses.Any(c => c.CourseId == courseId && c.IsPublished))
                return new PaginationResult<IEnumerable<CourseRevenueByClubResponseDto>>([], 0, page, pageSize);

            var products = await _unitOfWork.Products.GetManyByCondition(p => p.ReferenceID == courseId);
            var productIds = products.Select(p => p.ProductID).ToHashSet();

            if (!productIds.Any())
                return new PaginationResult<IEnumerable<CourseRevenueByClubResponseDto>>([], 0, page, pageSize);

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            var courseOrders = allOrders?
                .Where(o => o.Item != null && productIds.Contains(o.Item.ProductID))
                .ToList() ?? new List<Domain.Entities.Mongo.Order>();

            if (!courseOrders.Any())
                return new PaginationResult<IEnumerable<CourseRevenueByClubResponseDto>>([], 0, page, pageSize);

            var clubStats = courseOrders
                .GroupBy(o => o.ClubID)
                .Select(g => new
                {
                    ClubId = g.Key,
                    TotalRevenue = g.Sum(o => o.TotalAmount),
                    TotalLearners = g.Select(o => o.UserID).Distinct().Count()
                })
                .ToList();

            var clubIds = clubStats.Select(s => s.ClubId).ToList();
            var clubs = await _unitOfWork.Clubs.GetManyByCondition(c => clubIds.Contains(c.ClubID));
            var clubDict = clubs.ToDictionary(c => c.ClubID, c => c);

            var result = clubStats
                .Where(s => clubDict.ContainsKey(s.ClubId))
                .Select(s =>
                {
                    var club = clubDict[s.ClubId];
                    return new CourseRevenueByClubResponseDto
                    {
                        ClubId = club.ClubID,
                        ClubNameVN = club.NameVN,
                        ClubNameEN = club.NameEN,
                        ImageUrl = club.ImageUrl,
                        TotalLearners = s.TotalLearners,
                        TotalRevenue = s.TotalRevenue
                    };
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            var totalRecords = result.Count;
            var paged = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PaginationResult<IEnumerable<CourseRevenueByClubResponseDto>>(paged, totalRecords, page, pageSize);
        }

        public async Task<PaginationResult<IEnumerable<ClubDashboardResponseDto>>> GetClubDashboardAsync(int page = 1, int pageSize = 10)
        {
            var clubs = await _unitOfWork.Clubs.GetAll();
            var participations = await _unitOfWork.Participations.GetAll();
            
            var memberCounts = participations
                .GroupBy(p => p.ClubID)
                .ToDictionary(g => g.Key, g => g.Count());

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            var clubRevenues = allOrders != null 
                ? allOrders
                    .Where(o => o.OrderType == OrderType.USER_PURCHASE)
                    .GroupBy(o => o.ClubID)
                    .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount))
                : [];

            var dashboardClubs = clubs.Select(c => new ClubDashboardResponseDto
            {
                ClubId = c.ClubID,
                ClubNameVN = c.NameVN,
                ClubNameEN = c.NameEN,
                ImageUrl = c.ImageUrl,
                TotalMembers = memberCounts.TryGetValue(c.ClubID, out var count) ? count : 0,
                TotalRevenue = clubRevenues.TryGetValue(c.ClubID, out var rev) ? rev : 0m,
                Status = c.Status
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToList();

            var totalRecords = dashboardClubs.Count;
            var pagedResult = dashboardClubs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PaginationResult<IEnumerable<ClubDashboardResponseDto>>(pagedResult, totalRecords, page, pageSize);
        }

        public async Task<PaginationResult<IEnumerable<ClubMemberTransactionResponseDto>>> GetClubMemberTransactionsAsync(Guid clubId, Guid? courseId, int page = 1, int pageSize = 10)
        {
            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            var query = allOrders?
                .Where(o => o.ClubID == clubId && o.OrderType == OrderType.USER_PURCHASE && o.Item != null && o.Item.Type == ProductType.COURSE)
                .ToList() ?? new List<Domain.Entities.Mongo.Order>();

            if (courseId.HasValue && courseId != Guid.Empty)
            {
                var products = await _unitOfWork.Products.GetManyByCondition(p => p.ReferenceID == courseId.Value);
                var productIds = products.Select(p => p.ProductID).ToList();
                query = query.Where(o => productIds.Contains(o.Item!.ProductID)).ToList();
            }

            var orderedTransactions = query.OrderByDescending(o => o.Payment?.TransactionDate ?? o.CreateAt).ToList();
            var totalRecords = orderedTransactions.Count;
            var pagedTransactions = orderedTransactions.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            if (!pagedTransactions.Any())
                return new PaginationResult<IEnumerable<ClubMemberTransactionResponseDto>>([], totalRecords, page, pageSize);

            var userIds = pagedTransactions.Select(o => o.UserID).Distinct().ToList();
            var users = await _identityMicroserviceClient.GetUsersBulk(userIds);
            var userDict = users.ToDictionary(u => u.UserId, u => new SimpleUserReponse
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.LastName + " " + u.FirstName,
                AvatarUrl = u.ImageUrl
            });

            var productIdsInPage = pagedTransactions.Select(o => o.Item!.ProductID).Distinct().ToList();
            var productsInPage = await _unitOfWork.Products.GetManyByCondition(p => productIdsInPage.Contains(p.ProductID));
            var productToCourseDict = productsInPage.ToDictionary(p => p.ProductID, p => p.ReferenceID);

            var courses = await _academyClient.GetCourseDashboard();
            var courseDict = courses.ToDictionary(c => c.CourseId, c => c);

            var result = pagedTransactions.Select(o =>
            {
                var productId = o.Item!.ProductID;
                var cId = productToCourseDict.TryGetValue(productId, out var refId) ? refId : Guid.Empty;
                var courseImg = courseDict.TryGetValue(cId, out var course) ? course.ImageUrl : null;

                return new ClubMemberTransactionResponseDto
                {
                    User = userDict.TryGetValue(o.UserID, out var u) ? u : new SimpleUserReponse { UserId = o.UserID, Email = "", FullName = "Unknown" },
                    CourseId = cId,
                    CourseNameVN = o.Item.ProductNameVN,
                    CourseNameEN = o.Item.ProductNameEN,
                    CourseImageUrl = courseImg,
                    Amount = o.TotalAmount,
                    TransactionDate = o.Payment?.TransactionDate ?? o.CreateAt
                };
            }).ToList();

            return new PaginationResult<IEnumerable<ClubMemberTransactionResponseDto>>(result, totalRecords, page, pageSize);
        }
    }
}
