using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.AppHelpers;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly IClock _clock;

        public DashboardService(
            IUnitOfWork unitOfWork,
            IOrderRepository orderRepository,
            AcademyMicroserviceClient academyMicroserviceClient,
            IdentityMicroserviceClient identityMicroserviceClient,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _academyMicroserviceClient = academyMicroserviceClient;
            _identityMicroserviceClient = identityMicroserviceClient;
            _clock = clock;
        }

        public async Task<RevenueOverviewResponse> GetRevenueOverviewByClub(Guid clubId)
        {
            var clubExists = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId, q => q.AsNoTracking());
            if (clubExists == null)
                throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

            var now = _clock.Now;
            var startThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startNextMonth = startThisMonth.AddMonths(1);
            var startLastMonth = startThisMonth.AddMonths(-1);

            var aggregate = await _orderRepository.GetRevenueOverviewOrderAggregateByClubId(
                clubId,
                startLastMonth,
                startThisMonth,
                startNextMonth);

            return new RevenueOverviewResponse
            {
                TotalExpense = aggregate.TotalExpense,
                ExpenseThisMonth = aggregate.ExpenseThisMonth,
                ExpenseLastMonth = aggregate.ExpenseLastMonth,

                TotalTransactions = aggregate.TotalTransactions,
                TransactionsThisMonth = aggregate.TransactionsThisMonth
            };
        }

        public async Task<RevenueGrowthResponse> GetRevenueGrowthByClub(Guid clubId, int months)
        {
            if (months <= 0)
                months = 12;

            var clubExists = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId, q => q.AsNoTracking());
            if (clubExists == null)
                throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

            var now = _clock.Now;
            var startCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var fromMonth = startCurrentMonth.AddMonths(-(months - 1));
            var toExclusive = startCurrentMonth.AddMonths(1);

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null)
                allOrders = [];

            // Filter for CLUB_IMPORT orders for this club (club's spending/expense)
            var clubImportOrders = allOrders
                .Where(o => o.ClubID == clubId 
                    && o.OrderType == Domain.Enums.OrderType.CLUB_IMPORT
                    && o.Payment?.TransactionDate >= fromMonth
                    && o.Payment?.TransactionDate < toExclusive)
                .ToList();

            var valueByMonth = clubImportOrders
                .GroupBy(x => new DateTime(x.Payment.TransactionDate.Year, x.Payment.TransactionDate.Month, 1))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

            var growth = Enumerable.Range(0, months)
                .Select(i => fromMonth.AddMonths(i))
                .Select(m => new MonthlyStat
                {
                    Month = m,
                    Value = valueByMonth.TryGetValue(m, out var value) ? value : 0
                })
                .ToList();

            var totalValue = growth.Sum(x => x.Value);
            var firstValue = growth.FirstOrDefault()?.Value ?? 0;
            var lastValue = growth.LastOrDefault()?.Value ?? 0;
            var growthRate = (decimal)CalculateGrowthRate(lastValue, firstValue);

            return new RevenueGrowthResponse 
            { 
                RevenueGrowth = growth,
                TotalValue = totalValue,
                GrowthRate = growthRate
            };
        }

        /// <summary>
        /// Lấy biểu đồ tăng doanh thu theo ngày của câu lạc bộ trong khoảng thời gian cụ thể.
        /// </summary>
        public async Task<RevenueGrowthResponse> GetRevenueGrowthByClub(Guid clubId, DateTime fromDate, DateTime toDate)
        {
            var clubExists = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId, q => q.AsNoTracking());
            if (clubExists == null)
                throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

            // Normalize dates to start and end of day
            var from = fromDate.Date;
            var to = toDate.Date.AddDays(1); // Inclusive of toDate

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null)
                allOrders = [];

            // Filter for CLUB_IMPORT orders for this club (club's spending/expense) by date range
            var clubImportOrders = allOrders
                .Where(o => o.ClubID == clubId 
                    && o.OrderType == Domain.Enums.OrderType.CLUB_IMPORT
                    && o.Payment?.TransactionDate >= from
                    && o.Payment?.TransactionDate < to)
                .ToList();

            // Group by date
            var valueByDate = clubImportOrders
                .GroupBy(x => x.Payment.TransactionDate.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

            // Generate daily stats for the entire date range
            var dailyStats = new List<MonthlyStat>();
            for (var date = from; date < to; date = date.AddDays(1))
            {
                dailyStats.Add(new MonthlyStat
                {
                    Month = date, // Using Month field for date (for compatibility)
                    Value = valueByDate.TryGetValue(date, out var value) ? value : 0
                });
            }

            var totalValue = dailyStats.Sum(x => x.Value);
            var firstValue = dailyStats.FirstOrDefault()?.Value ?? 0;
            var lastValue = dailyStats.LastOrDefault()?.Value ?? 0;
            var growthRate = (decimal)CalculateGrowthRate(lastValue, firstValue);

            return new RevenueGrowthResponse 
            { 
                RevenueGrowth = dailyStats,
                TotalValue = totalValue,
                GrowthRate = growthRate
            };
        }

        public async Task<ClubCourseRevenueResponse> GetRevenueByCourseByClub(Guid clubId, int top)
        {
            if (top <= 0)
                top = 10;

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null)
                allOrders = [];

            // Lấy USER_PURCHASE orders của club (người dùng mua khóa học từ club = doanh thu theo khóa học)
            var userPurchaseOrders = allOrders
                .Where(o => o.ClubID == clubId
                    && o.OrderType == Domain.Enums.OrderType.USER_PURCHASE
                    && o.Item != null
                    && o.Item.ProductID != Guid.Empty)
                .ToList();

            if (userPurchaseOrders.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            // Lấy ProductID duy nhất rồi query SQL một lần
            var productIds = userPurchaseOrders
                .Select(o => o.Item.ProductID)
                .Distinct()
                .ToList();

            var products = await _unitOfWork.Products
                .GetManyByCondition(p => productIds.Contains(p.ProductID));

            var courseIdByProductId = products
                .Where(p => p.ReferenceID != Guid.Empty)
                .ToDictionary(p => p.ProductID, p => p.ReferenceID);

            if (courseIdByProductId.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var revenueByCourseId = userPurchaseOrders
                .Where(o => courseIdByProductId.ContainsKey(o.Item.ProductID))
                .GroupBy(o => courseIdByProductId[o.Item.ProductID])
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

            if (revenueByCourseId.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var academyCourses = await _academyMicroserviceClient.GetCoursesByIdsSimple(clubId);
            var courseById = academyCourses
                .GroupBy(c => c.CourseId)
                .ToDictionary(g => g.Key, g => g.First());

            var stats = revenueByCourseId
                .Where(kvp => courseById.ContainsKey(kvp.Key))
                .Select(kvp => new CourseRevenueStat
                {
                    CourseInfo = courseById[kvp.Key],
                    Revenue = kvp.Value
                })
                .OrderByDescending(x => x.Revenue)
                .Take(top)
                .ToList();

            return new ClubCourseRevenueResponse { RevenueByCourse = stats };
        }

        private async Task<(List<Guid> ProductIds, Dictionary<Guid, Guid> CourseIdByProductId)> GetClubProductContext(Guid clubId)
        {
            // Lấy tất cả đơn hàng USER_PURCHASE thành công của club để lấy danh sách ProductID
            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            var productIds = (allOrders ?? [])
                .Where(o => o.ClubID == clubId
                    && o.OrderType == Domain.Enums.OrderType.USER_PURCHASE
                    && o.Item != null
                    && o.Item.ProductID != Guid.Empty)
                .Select(o => o.Item.ProductID)
                .Distinct()
                .ToList();

            if (productIds.Count == 0)
                return ([], []);

            // Query bảng Product (SQL) để lấy ReferenceID = CourseID
            var products = await _unitOfWork.Products
                .GetManyByCondition(p => productIds.Contains(p.ProductID));

            var courseIdByProductId = products
                .Where(p => p.ReferenceID != Guid.Empty)
                .ToDictionary(p => p.ProductID, p => p.ReferenceID);

            return (productIds, courseIdByProductId);
        }

        private static double CalculateGrowthRate(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0)
                return currentValue > 0 ? 100 : 0;

            return Math.Round((double)((currentValue - previousValue) / previousValue * 100), 2);
        }

        public async Task<AdminRevenueOverviewResponse> GetAdminRevenueOverview()
        {
            // Get timestamp ranges
            var now = _clock.Now;
            var startThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startNextMonth = startThisMonth.AddMonths(1);
            var startLastMonth = startThisMonth.AddMonths(-1);

            var today = now.Date;
            var startThisWeek = today.AddDays(-(int)today.DayOfWeek);
            var startLastWeek = startThisWeek.AddDays(-7);

            var startThisYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var startLastYear = startThisYear.AddYears(-1);

            // Get all orders (including pending/failed) for success rate calculation
            var allSystemOrders = await _orderRepository.GetAllOrders();
            if (allSystemOrders == null) allSystemOrders = [];
            var allSystemOrdersList = allSystemOrders.ToList();

            // Get all successful orders across all clubs
            var allOrdersList = allSystemOrdersList.Where(o => o.Status == OrderStatus.SUCCESS).ToList();

            // For admin overview, revenue/profit are based on all successful orders in the system.
            var totalRevenue = allOrdersList.Sum(o => o.TotalAmount);
            var revenueThisMonth = allOrdersList
                .Where(o => o.Payment != null 
                    && o.Payment.TransactionDate >= startThisMonth 
                    && o.Payment.TransactionDate < startNextMonth)
                .Sum(o => o.TotalAmount);
            var revenueLastMonth = allOrdersList
                .Where(o => o.Payment != null 
                    && o.Payment.TransactionDate >= startLastMonth 
                    && o.Payment.TransactionDate < startThisMonth)
                .Sum(o => o.TotalAmount);

            var netProfit = allOrdersList.Sum(o => o.TotalAmount);
            var profitThisMonth = allOrdersList
                .Where(o => o.Payment != null 
                    && o.Payment.TransactionDate >= startThisMonth 
                    && o.Payment.TransactionDate < startNextMonth)
                .Sum(o => o.TotalAmount);
            var profitLastMonth = allOrdersList
                .Where(o => o.Payment != null 
                    && o.Payment.TransactionDate >= startLastMonth 
                    && o.Payment.TransactionDate < startThisMonth)
                .Sum(o => o.TotalAmount);

            // Calculate growth rates
            var revenueGrowthRate = CalculateGrowthRate(revenueThisMonth, revenueLastMonth);
            var profitGrowthRate = CalculateGrowthRate(profitThisMonth, profitLastMonth);

            // Count transactions - all successful orders regardless of type
            var totalTransactions = allOrdersList.Count;
            var transactionsThisMonth = allOrdersList
                .Where(o => o.Payment != null 
                    && o.Payment.TransactionDate >= startThisMonth 
                    && o.Payment.TransactionDate < startNextMonth)
                .Count();

            // Expanded KPIs
            var totalOrdersCount = allSystemOrdersList.Count;
            var successRate = totalOrdersCount > 0 ? (double)allOrdersList.Count / totalOrdersCount * 100 : 0;
            var pendingRefunds = allSystemOrdersList.Count(o => o.Status == OrderStatus.PENDING_REFUND);

            var revenueToday = allOrdersList
                .Where(o => o.Payment != null && o.Payment.TransactionDate.Date == today)
                .Sum(o => o.TotalAmount);
            var revenueYesterday = allOrdersList
                .Where(o => o.Payment != null && o.Payment.TransactionDate.Date == today.AddDays(-1))
                .Sum(o => o.TotalAmount);

            var revenueThisWeek = allOrdersList
                .Where(o => o.Payment != null && o.Payment.TransactionDate.Date >= startThisWeek && o.Payment.TransactionDate.Date <= today)
                .Sum(o => o.TotalAmount);
            var revenueLastWeek = allOrdersList
                .Where(o => o.Payment != null && o.Payment.TransactionDate.Date >= startLastWeek && o.Payment.TransactionDate.Date < startThisWeek)
                .Sum(o => o.TotalAmount);

            var revenueThisYear = allOrdersList
                .Where(o => o.Payment != null && o.Payment.TransactionDate.Date >= startThisYear && o.Payment.TransactionDate.Date <= today)
                .Sum(o => o.TotalAmount);
            var revenueLastYear = allOrdersList
                .Where(o => o.Payment != null && o.Payment.TransactionDate.Date >= startLastYear && o.Payment.TransactionDate.Date < startThisYear)
                .Sum(o => o.TotalAmount);

            var transactionsLastMonth = allOrdersList
                .Where(o => o.Payment != null 
                    && o.Payment.TransactionDate >= startLastMonth 
                    && o.Payment.TransactionDate < startThisMonth)
                .Count();

            return new AdminRevenueOverviewResponse
            {
                TotalRevenue = totalRevenue,
                RevenueThisMonth = revenueThisMonth,
                RevenueLastMonth = revenueLastMonth,
                RevenueGrowthRate = revenueGrowthRate,

                NetProfit = netProfit,
                ProfitThisMonth = profitThisMonth,
                ProfitLastMonth = profitLastMonth,
                ProfitGrowthRate = profitGrowthRate,

                TotalTransactions = totalTransactions,
                TransactionsThisMonth = transactionsThisMonth,

                SuccessRate = successRate,
                PendingRefunds = pendingRefunds,

                RevenueToday = revenueToday,
                RevenueYesterday = revenueYesterday,
                RevenueThisWeek = revenueThisWeek,
                RevenueLastWeek = revenueLastWeek,
                RevenueThisYear = revenueThisYear,
                RevenueLastYear = revenueLastYear,

                TransactionsLastMonth = transactionsLastMonth
            };
        }

        public async Task<RevenueGrowthResponse> GetRevenueGrowthByAllClubs(int months)
        {
            if (months <= 0)
                months = 12;

            var now = _clock.Now;
            var startCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var fromMonth = startCurrentMonth.AddMonths(-(months - 1));
            var toExclusive = startCurrentMonth.AddMonths(1);

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null)
                allOrders = [];

            var valueByMonth = allOrders
                .Where(o => o.Payment != null
                    && o.Payment.TransactionDate >= fromMonth
                    && o.Payment.TransactionDate < toExclusive)
                .GroupBy(o => new DateTime(o.Payment.TransactionDate.Year, o.Payment.TransactionDate.Month, 1))
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

            var growth = Enumerable.Range(0, months)
                .Select(i => fromMonth.AddMonths(i))
                .Select(m => new MonthlyStat
                {
                    Month = m,
                    Value = valueByMonth.TryGetValue(m, out var value) ? value : 0
                })
                .ToList();

            var totalValue = growth.Sum(x => x.Value);
            var firstValue = growth.FirstOrDefault()?.Value ?? 0;
            var lastValue = growth.LastOrDefault()?.Value ?? 0;
            var growthRate = (decimal)CalculateGrowthRate(lastValue, firstValue);

            return new RevenueGrowthResponse 
            { 
                RevenueGrowth = growth,
                TotalValue = totalValue,
                GrowthRate = growthRate
            };
        }

        /// <summary>
        /// Lấy biểu đồ tăng doanh thu theo ngày của toàn bộ hệ thống trong khoảng thời gian cụ thể.
        /// </summary>
        public async Task<RevenueGrowthResponse> GetRevenueGrowthByAllClubs(DateTime fromDate, DateTime toDate)
        {
            // Normalize dates to start and end of day
            var from = fromDate.Date;
            var to = toDate.Date.AddDays(1); // Inclusive of toDate

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null)
                allOrders = [];

            // Filter all orders by date range
            var filteredOrders = allOrders
                .Where(o => o.Payment != null
                    && o.Payment.TransactionDate >= from
                    && o.Payment.TransactionDate < to)
                .ToList();

            // Group by date
            var valueByDate = filteredOrders
                .GroupBy(o => o.Payment.TransactionDate.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

            // Generate daily stats for the entire date range
            var dailyStats = new List<MonthlyStat>();
            for (var date = from; date < to; date = date.AddDays(1))
            {
                dailyStats.Add(new MonthlyStat
                {
                    Month = date, // Using Month field for date (for compatibility)
                    Value = valueByDate.TryGetValue(date, out var value) ? value : 0
                });
            }

            var totalValue = dailyStats.Sum(x => x.Value);
            var firstValue = dailyStats.FirstOrDefault()?.Value ?? 0;
            var lastValue = dailyStats.LastOrDefault()?.Value ?? 0;
            var growthRate = (decimal)CalculateGrowthRate(lastValue, firstValue);

            return new RevenueGrowthResponse 
            { 
                RevenueGrowth = dailyStats,
                TotalValue = totalValue,
                GrowthRate = growthRate
            };
        }

        public async Task<ClubCourseRevenueResponse> GetRevenueByCourseByAllClubs(int top)
        {
            if (top <= 0)
                top = 10;

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null)
                allOrders = [];

            // Lấy tất cả USER_PURCHASE orders (người dùng mua khóa học = doanh thu hệ thống)
            var userPurchaseOrders = allOrders
                .Where(o => o.OrderType == Domain.Enums.OrderType.USER_PURCHASE
                    && o.Item != null
                    && o.Item.ProductID != Guid.Empty)
                .ToList();

            if (userPurchaseOrders.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            // Lấy tất cả ProductID duy nhất từ orders
            var allProductIds = userPurchaseOrders
                .Select(o => o.Item.ProductID)
                .Distinct()
                .ToList();

            // Query một lần duy nhất từ DB để lấy mapping ProductID -> CourseID (ReferenceID)
            var products = await _unitOfWork.Products
                .GetManyByCondition(p => allProductIds.Contains(p.ProductID));

            var allCourseIdByProductId = products
                .Where(p => p.ReferenceID != Guid.Empty)
                .ToDictionary(p => p.ProductID, p => p.ReferenceID);

            if (allCourseIdByProductId.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var revenueByCourseId = userPurchaseOrders
                .Where(o => allCourseIdByProductId.ContainsKey(o.Item.ProductID))
                .GroupBy(o => allCourseIdByProductId[o.Item.ProductID])
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

            if (revenueByCourseId.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            // Lấy danh sách club liên quan để fetch course info
            var clubIds = userPurchaseOrders
                .Select(o => o.ClubID)
                .Distinct()
                .ToList();

            var allCourses = new List<SimpleCourseResponse>();
            foreach (var clubId in clubIds)
            {
                var coursesByClub = await _academyMicroserviceClient.GetCoursesByIdsSimple(clubId);
                allCourses.AddRange(coursesByClub);
            }

            var courseById = allCourses
                .GroupBy(c => c.CourseId)
                .ToDictionary(g => g.Key, g => g.First());

            var stats = revenueByCourseId
                .Where(kvp => courseById.ContainsKey(kvp.Key))
                .Select(kvp => new CourseRevenueStat
                {
                    CourseInfo = courseById[kvp.Key],
                    Revenue = kvp.Value
                })
                .OrderByDescending(x => x.Revenue)
                .Take(top)
                .ToList();

            return new ClubCourseRevenueResponse { RevenueByCourse = stats };
        }

        public async Task<AdminClubRankingResponse> GetAdminClubRankingBySpent(int top = 10)
        {
            if (top <= 0)
                top = 10;

            // Get all clubs
            var clubList = await _unitOfWork.Clubs.GetAll();
            if (clubList == null || !clubList.Any())
                return new AdminClubRankingResponse { Clubs = [] };

            // Get all orders with CLUB_IMPORT type and SUCCESS status
            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null || !allOrders.Any())
                return new AdminClubRankingResponse { Clubs = [] };

            // Group all successful orders by ClubID
            var ordersByClub = allOrders
                .GroupBy(o => o.ClubID)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Build ranking items
            var rankingItems = new List<ClubRankingItem>();

            foreach (var club in clubList.Where(c => ordersByClub.ContainsKey(c.ClubID)))
            {
                var clubOrders = ordersByClub[club.ClubID];
                var totalSpent = clubOrders.Sum(o => o.TotalAmount);
                var transactionCount = clubOrders.Count;

                // Get product IDs and map to course IDs
                var productIds = clubOrders
                    .Select(o => o.Item?.ProductID)
                    .Where(pid => pid.HasValue && pid.Value != Guid.Empty)
                    .Select(pid => pid.Value)
                    .Distinct()
                    .ToList();

                var courses = new List<SimpleCourseResponse>();

                if (productIds.Count > 0)
                {
                    try
                    {
                        // Get product context to map ProductID -> CourseID
                        var (_, courseIdByProductId) = await GetClubProductContext(club.ClubID);

                        // Get course IDs from the products in this club's orders
                        var courseIds = productIds
                            .Where(pid => courseIdByProductId.ContainsKey(pid))
                            .Select(pid => courseIdByProductId[pid])
                            .Distinct()
                            .ToList();

                        if (courseIds.Count > 0)
                        {
                            // Fetch course details from Academy service
                            var academyCourses = await _academyMicroserviceClient.GetCoursesByIdsSimple(club.ClubID);
                            courses = academyCourses
                                .Where(c => courseIds.Contains(c.CourseId))
                                .ToList();
                        }
                    }
                    catch
                    {
                        // If course fetching fails, continue with empty courses list
                        courses = [];
                    }
                }

                rankingItems.Add(new ClubRankingItem
                {
                    ClubID = club.ClubID,
                    NameVN = club.NameVN,
                    NameEN = club.NameEN,
                    ImageUrl = club.ImageUrl,
                    ClubCode = club.ClubCode,
                    TotalSpent = totalSpent,
                    TransactionCount = transactionCount,
                    Courses = courses
                });
            }

            // Sort by TotalSpent descending and take top N
            var result = rankingItems
                .OrderByDescending(x => x.TotalSpent)
                .Take(top)
                .ToList();

            return new AdminClubRankingResponse { Clubs = result };
        }

        // ===================== Competition Stats =====================

        public async Task<CompetitionStatsResponse> GetCompetitionStats(int top = 10, CompetitionFilterRequest? filter = null)
        {
            if (top <= 0) top = 10;

            // Lấy tất cả competitions
            var allCompetitions = (await _unitOfWork.Competitions.GetAll())?.ToList() ?? [];
            if (allCompetitions.Count == 0)
                return new CompetitionStatsResponse
                {
                    Overview = new CompetitionOverviewResponse(),
                    TopByParticipants = []
                };

            return await BuildCompetitionStats(allCompetitions, top, filter);
        }

        public async Task<CompetitionStatsResponse> GetCompetitionStatsByClub(Guid clubId, int top = 10, CompetitionFilterRequest? filter = null)
        {
            if (top <= 0) top = 10;

            var clubExists = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId, q => q.AsNoTracking());
            if (clubExists == null)
                throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

            // Lấy competitions của club
            var clubCompetitions = (await _unitOfWork.Competitions
                .GetManyByCondition(c => c.ClubID == clubId))?.ToList() ?? [];

            if (clubCompetitions.Count == 0)
                return new CompetitionStatsResponse
                {
                    Overview = new CompetitionOverviewResponse(),
                    TopByParticipants = []
                };

            return await BuildCompetitionStats(clubCompetitions, top, filter);
        }

        /// <summary>
        /// Logic chung: build CompetitionStatsResponse từ danh sách competitions đã filter.
        /// </summary>
        private async Task<CompetitionStatsResponse> BuildCompetitionStats(
            List<Competition> competitions, int top, CompetitionFilterRequest? filter = null)
        {
            var now = _clock.Now;

            // Áp dụng filters trước khi tính toán statistics
            var filteredCompetitions = ApplyCompetitionFilters(competitions, filter, now);

            var competitionIds = filteredCompetitions.Select(c => c.CompetitionID).ToList();

            // Lấy số người tham gia ACTIVE cho tất cả filtered competitions một lần
            var participantCounts = await _unitOfWork.UserCompetitions
                .GetCompetitorCountsByCompetitionIds(competitionIds);

            // Nếu có filter về competitors, lọc thêm dựa trên participantCounts
            if (filter?.MinTotalCompetitors.HasValue == true || filter?.MaxTotalCompetitors.HasValue == true)
            {
                filteredCompetitions = filteredCompetitions
                    .Where(c =>
                    {
                        var count = participantCounts.TryGetValue(c.CompetitionID, out var cnt) ? cnt : 0;
                        var minOk = !filter.MinTotalCompetitors.HasValue || count >= filter.MinTotalCompetitors.Value;
                        var maxOk = !filter.MaxTotalCompetitors.HasValue || count <= filter.MaxTotalCompetitors.Value;
                        return minOk && maxOk;
                    })
                    .ToList();

                // Cập nhật lại competitionIds sau khi lọc
                competitionIds = filteredCompetitions.Select(c => c.CompetitionID).ToList();
                
                // Cập nhật participantCounts để chỉ chứa những ID đã lọc
                participantCounts = participantCounts
                    .Where(kvp => competitionIds.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            var totalParticipants = participantCounts.Values.Sum();
            var publishedCount = filteredCompetitions.Count(c => c.Status == CompetitionStatus.PUBLISHED);

            // Đang diễn ra: PUBLISHED + trong khoảng StartDate → EndDate
            var ongoingCount = filteredCompetitions.Count(c =>
                c.Status == CompetitionStatus.PUBLISHED
                && c.StartDate <= now
                && c.EndDate >= now);

            var overview = new CompetitionOverviewResponse
            {
                TotalCompetitions = filteredCompetitions.Count,
                DraftCompetitions = filteredCompetitions.Count(c => c.Status == CompetitionStatus.DRAFT),
                PublishedCompetitions = publishedCount,
                CompletedCompetitions = filteredCompetitions.Count(c => c.Status == CompetitionStatus.RESULT_PUBLISHED),
                CancelledCompetitions = filteredCompetitions.Count(c => c.Status == CompetitionStatus.CANCELLED),
                InvalidCompetitions = filteredCompetitions.Count(c => c.Status == CompetitionStatus.INVALID),
                TotalParticipants = totalParticipants,
                AverageParticipantsPerCompetition = filteredCompetitions.Count > 0
                    ? Math.Round((double)totalParticipants / filteredCompetitions.Count, 1)
                    : 0
            };

            // Lấy tên club cho hiển thị
            var clubIds = filteredCompetitions.Select(c => c.ClubID).Distinct().ToList();
            var clubs = (await _unitOfWork.Clubs
                .GetManyByCondition(c => clubIds.Contains(c.ClubID)))
                .ToDictionary(c => c.ClubID);

            // Top cuộc thi theo số người tham gia
            var topItems = filteredCompetitions
                .Select(c => new CompetitionStatItem
                {
                    CompetitionId = c.CompetitionID,
                    NameVN = c.NameVN,
                    NameEN = c.NameEN,
                    CompetitionStatus = c.Status,
                    CompetitionPhase = CommunityAppHelpers.GetCurrentCompetitionLifeCycle(c, now),
                    ClubId = c.ClubID,
                    ClubNameVN = clubs.TryGetValue(c.ClubID, out var club) ? club.NameVN : string.Empty,
                    ParticipantCount = participantCounts.TryGetValue(c.CompetitionID, out var count) ? count : 0,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate
                })
                .OrderByDescending(x => x.ParticipantCount)
                .Take(top)
                .ToList();

            return new CompetitionStatsResponse
            {
                Overview = overview,
                TopByParticipants = topItems
            };
        }

        /// <summary>
        /// Áp dụng tất cả filter conditions vào danh sách competitions.
        /// Sử dụng CommunityAppHelpers.GetCurrentCompetitionLifeCycle() để tính toán CompetitionPhase.
        /// </summary>
        private List<Competition> ApplyCompetitionFilters(
            List<Competition> competitions,
            CompetitionFilterRequest? filter,
            DateTime now)
        {
            if (filter == null)
                return competitions;

            var result = competitions.AsEnumerable();

            // Lọc theo CompetitionStatus
            if (filter.CompetitionStatus.HasValue)
            {
                result = result.Where(c => c.Status == filter.CompetitionStatus.Value);
            }

            // Lọc theo CompetitionPhase (giai đoạn vòng đời)
            if (filter.CompetitionPhase.HasValue)
            {
                result = result.Where(c =>
                {
                    var phase = CommunityAppHelpers.GetCurrentCompetitionLifeCycle(c, now);
                    return phase == filter.CompetitionPhase.Value;
                });
            }

            // Lọc theo ClubId
            if (filter.ClubId.HasValue)
            {
                result = result.Where(c => c.ClubID == filter.ClubId.Value);
            }

            // Lọc theo StartDate range
            if (filter.StartDateFrom.HasValue)
            {
                result = result.Where(c => c.StartDate >= filter.StartDateFrom.Value);
            }

            if (filter.StartDateTo.HasValue)
            {
                result = result.Where(c => c.StartDate <= filter.StartDateTo.Value);
            }

            // Lọc theo EndDate range
            if (filter.EndDateFrom.HasValue)
            {
                result = result.Where(c => c.EndDate >= filter.EndDateFrom.Value);
            }

            if (filter.EndDateTo.HasValue)
            {
                result = result.Where(c => c.EndDate <= filter.EndDateTo.Value);
            }

            // Lọc theo CreatedBy
            if (filter.CreatedBy.HasValue)
            {
                result = result.Where(c => c.CreatedBy == filter.CreatedBy.Value);
            }

            // Lọc theo UpdatedBy
            if (filter.UpdatedBy.HasValue)
            {
                result = result.Where(c => c.UpdatedBy == filter.UpdatedBy.Value);
            }

            // Lọc theo số vòng (Rounds count)
            if (filter.MinTotalRounds.HasValue)
            {
                result = result.Where(c => c.Rounds.Count >= filter.MinTotalRounds.Value);
            }

            if (filter.MaxTotalRounds.HasValue)
            {
                result = result.Where(c => c.Rounds.Count <= filter.MaxTotalRounds.Value);
            }

            // Lọc theo số giải thưởng (CompetitionPrizes count)
            if (filter.MinTotalPrizes.HasValue)
            {
                result = result.Where(c => c.CompetitionPrizes.Count >= filter.MinTotalPrizes.Value);
            }

            if (filter.MaxTotalPrizes.HasValue)
            {
                result = result.Where(c => c.CompetitionPrizes.Count <= filter.MaxTotalPrizes.Value);
            }

            // Note: Lọc theo MinTotalCompetitors và MaxTotalCompetitors được xử lý sau 
            // vì cần lấy dữ liệu từ UserCompetitions (sẽ được thực hiện trong BuildCompetitionStats)

            return result.ToList();
        }

        // ===================== Code Stats =====================

        public async Task<CodeStatsOverviewResponse> GetCodeStatsByClub(Guid clubId)
        {
            var clubExists = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId, q => q.AsNoTracking());
            if (clubExists == null)
                throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

            // Gọi Academy service để lấy code stats
            var codeStats = await _academyMicroserviceClient.GetCodeStatsByClub(clubId);
            return codeStats;
        }

        public async Task<CodeStatsOverviewResponse> GetCodeStatsAdmin()
        {
            // Gọi Academy service để lấy code stats toàn hệ thống
            var codeStats = await _academyMicroserviceClient.GetCodeStatsAdmin();
            return codeStats;
        }

        // ===================== Top Buyers =====================

        public async Task<TopBuyersResponse> GetTopBuyersByClub(Guid clubId, int top = 10)
        {
            if (top <= 0)
                top = 10;

            var clubExists = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId, q => q.AsNoTracking());
            if (clubExists == null)
                throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null || !allOrders.Any())
                return new TopBuyersResponse { Buyers = [], TotalSystemRevenue = 0 };

            // Lấy các đơn hàng USER_PURCHASE của club
            var clubUserPurchaseOrders = allOrders
                .Where(o => o.ClubID == clubId
                    && o.OrderType == Domain.Enums.OrderType.USER_PURCHASE)
                .ToList();

            if (!clubUserPurchaseOrders.Any())
                return new TopBuyersResponse { Buyers = [], TotalSystemRevenue = 0 };

            // Nhóm theo UserID để tính tổng chi tiêu và số lần mua
            var buyerStats = clubUserPurchaseOrders
                .GroupBy(o => o.UserID)
                .Select(g => new
                {
                    UserId = g.Key,
                    Email = g.First().UserEmail,
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    PurchaseCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(top)
                .ToList();

            var userIds = buyerStats.Select(b => b.UserId).Distinct().ToList();
            var users = await _identityMicroserviceClient.GetUsersBulk(userIds);
            var userDict = users.ToDictionary(u => u.UserId);

            var buyers = buyerStats
                .Select(b => 
                {
                    var user = userDict.GetValueOrDefault(b.UserId);
                    return new BuyerStatItem
                    {
                        UserId = b.UserId,
                        UserName = user?.Username ?? string.Empty,
                        Email = user?.Email ?? b.Email ?? string.Empty,
                        ImageUrl = user?.ImageUrl,
                        TotalSpent = b.TotalSpent,
                        PurchaseCount = b.PurchaseCount
                    };
                })
                .ToList();

            var clubRevenue = clubUserPurchaseOrders.Sum(o => o.TotalAmount);

            return new TopBuyersResponse
            {
                Buyers = buyers,
                TotalSystemRevenue = clubRevenue
            };
        }

        public async Task<TopBuyersResponse> GetTopBuyersAdmin(int top = 10)
        {
            if (top <= 0)
                top = 10;

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null || !allOrders.Any())
                return new TopBuyersResponse { Buyers = [], TotalSystemRevenue = 0 };

            // Lấy tất cả đơn hàng USER_PURCHASE của toàn hệ thống
            var allUserPurchaseOrders = allOrders
                .Where(o => o.OrderType == Domain.Enums.OrderType.USER_PURCHASE)
                .ToList();

            if (!allUserPurchaseOrders.Any())
                return new TopBuyersResponse { Buyers = [], TotalSystemRevenue = 0 };

            // Nhóm theo UserID để tính tổng chi tiêu và số lần mua
            var buyerStats = allUserPurchaseOrders
                .GroupBy(o => o.UserID)
                .Select(g => new
                {
                    UserId = g.Key,
                    Email = g.First().UserEmail,
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    PurchaseCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(top)
                .ToList();

            var userIds = buyerStats.Select(b => b.UserId).Distinct().ToList();
            var users = await _identityMicroserviceClient.GetUsersBulk(userIds);
            var userDict = users.ToDictionary(u => u.UserId);

            var buyers = buyerStats
                .Select(b => 
                {
                    var user = userDict.GetValueOrDefault(b.UserId);
                    return new BuyerStatItem
                    {
                        UserId = b.UserId,
                        UserName = user?.Username ?? string.Empty,
                        Email = user?.Email ?? b.Email ?? string.Empty,
                        ImageUrl = user?.ImageUrl,
                        TotalSpent = b.TotalSpent,
                        PurchaseCount = b.PurchaseCount
                    };
                })
                .ToList();

            var totalSystemRevenue = allUserPurchaseOrders.Sum(o => o.TotalAmount);

            return new TopBuyersResponse
            {
                Buyers = buyers,
                TotalSystemRevenue = totalSystemRevenue
            };
        }

        // ===================== System Operations Management =====================

        public async Task<SystemTransactionLogsResponse> GetSystemTransactionLogs(int page = 1, int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit <= 0) limit = 10;

            var allOrders = await _orderRepository.GetAllOrders();
            if (allOrders == null) allOrders = [];

            var totalRecords = allOrders.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)limit);

            // Lọc và sắp xếp theo ngày tạo mới nhất, phân trang
            var pagedOrders = allOrders
                .OrderByDescending(o => o.CreateAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            var logs = pagedOrders.Select(o => new SystemTransactionLogEntry
            {
                OrderID = o._id.ToString(),
                UserName = o.UserName ?? "Unknown",
                Email = o.UserEmail ?? "Unknown",
                ProductName = string.IsNullOrWhiteSpace(o.Item?.ProductNameVN) ? "Unknown Product" : o.Item.ProductNameVN,
                Amount = o.TotalAmount,
                Status = o.Status.ToString(),
                PaymentMethod = "VNPAY", // Default hoặc lấy từ entity nếu có: o.Payment?.PaymentGateway ?? "VNPAY"
                CreatedAt = o.CreateAt
            }).ToList();

            return new SystemTransactionLogsResponse
            {
                Data = logs,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        public async Task<SystemOperationsSummaryResponse> GetSystemOperationsSummary(string identityFilterTimeLine = "month")
        {
            // 1. Pending Club Approvals
            var clubRequests = await _unitOfWork.ClubCreationRequests.GetAll();
            var pendingClubApprovals = clubRequests?.Count(r => r.Status == Domain.Enums.ClubCreationRequestStatus.PENDING) ?? 0;

            // 2. Call Identity service summary API and filter-time-lines
            IdentityUserSummaryResponse? identitySummary = null;
            var timelineOptions = await GetSystemFilterTimeLines();
            try
            {
                identitySummary = await _identityMicroserviceClient.GetUserSummaryAsync(identityFilterTimeLine);
            }
            catch (Exception ex)
            {
                // If Identity service fails, fall back to older approach
                _ = ex; // swallow, we'll fallback below
            }

            var totalUsers = identitySummary?.TotalUser ?? await _identityMicroserviceClient.GetTotalUserCount();
            var newUsersThisMonth = identitySummary?.NewUsers ?? 0;
            var memberCount = identitySummary?.MemberCount ?? 0;
            var clubOwnerCount = identitySummary?.ClubOwnerCount ?? 0;

            // If Identity summary didn't provide breakdowns, fallback to fetching users and counting roles
            if (identitySummary == null)
            {
                try
                {
                    var users = await _identityMicroserviceClient.GetAllUsers();
                    if (users != null && users.Count > 0)
                    {
                        memberCount = users.Count(u => u.RoleName == Droniverse.Shared.Constants.Roles.ClubMember);
                        clubOwnerCount = users.Count(u => u.RoleName == Droniverse.Shared.Constants.Roles.ClubManager);
                    }
                }
                catch (Exception)
                {
                    // ignore, keep previous values
                }
            }

            return new SystemOperationsSummaryResponse
            {
                PendingClubApprovals = pendingClubApprovals,
                TotalUsers = totalUsers,
                NewUsersThisMonth = newUsersThisMonth,
                MemberCount = memberCount,
                ClubOwnerCount = clubOwnerCount,
                FilterTimeLines = timelineOptions
            };
        }

        public async Task<IEnumerable<IdentityTimelineOptionDto>> GetSystemFilterTimeLines()
        {
            try
            {
                var optionsFromIdentity = await _identityMicroserviceClient.GetFilterTimeLinesAsync();
                if (optionsFromIdentity != null && optionsFromIdentity.Any())
                {
                    return optionsFromIdentity;
                }
            }
            catch
            {
                // fallback to static options below
            }

            return
            [
                new IdentityTimelineOptionDto("day", "Hom nay"),
                new IdentityTimelineOptionDto("week", "Tuan nay"),
                new IdentityTimelineOptionDto("last_week", "Tuan truoc"),
                new IdentityTimelineOptionDto("month", "Thang nay"),
                new IdentityTimelineOptionDto("last_month", "Thang truoc"),
                new IdentityTimelineOptionDto("month:2026-03", "Thang 3/2026"),
                new IdentityTimelineOptionDto("month:2026-04", "Thang 4/2026"),
                new IdentityTimelineOptionDto("year", "Nam nay")
            ];
        }

        public async Task<UserGrowthTrendResponse> GetUserGrowthTrend(int months = 12)
        {
            if (months <= 0) months = 12;

            var users = new List<Droniverse.Shared.DTOs.Response.UserResponse>();
            try
            {
                var fetchedUsers = await _identityMicroserviceClient.GetAllUsers();
                if (fetchedUsers != null)
                    users = fetchedUsers;
            }
            catch (Exception ex)
            {
                // Failed to fetch users for growth trend calculation
            }

            var now = _clock.Now;
            var startCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var fromMonth = startCurrentMonth.AddMonths(-(months - 1));
            var toExclusive = startCurrentMonth.AddMonths(1);

            // Temporarily ignore creation date since UserResponse does not include CreatedAt yet
            var usersByMonth = users
                .GroupBy(u => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc))
                .ToDictionary(g => g.Key, g => g.Count());

            var growth = Enumerable.Range(0, months)
                .Select(i => fromMonth.AddMonths(i))
                .Select(m => new MonthlyUserStat
                {
                    Month = m,
                    Value = usersByMonth.TryGetValue(m, out var value) ? value : 0
                })
                .ToList();

            return new UserGrowthTrendResponse
            {
                UserGrowth = growth
            };
        }

        public async Task<RecentActivityFeedResponse> GetRecentActivityFeed()
        {
            var activities = new List<ActivityFeedItem>();
            var now = _clock.Now;

            // 1. Transactions (Orders)
            var recentOrders = await _orderRepository.GetAllOrders();
            if (recentOrders != null)
            {
                activities.AddRange(recentOrders
                    .OrderByDescending(o => o.CreateAt)
                    .Take(10)
                    .Select(o => new ActivityFeedItem
                    {
                        ActivityType = "NEW_TRANSACTION",
                        Message = $"Giao dịch mới trị giá {o.TotalAmount:N0}đ từ User {o.UserName}",
                        Timestamp = o.CreateAt
                    }));
            }

            // 2. Club Creations
            var recentClubs = await _unitOfWork.ClubCreationRequests.GetAll();
            if (recentClubs != null)
            {
                activities.AddRange(recentClubs
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .Select(c => new ActivityFeedItem
                    {
                        ActivityType = "CLUB_REQUEST",
                        Message = $"CLB {c.NameVN} vừa nộp yêu cầu tạo mới",
                        Timestamp = c.CreatedAt
                    }));
            }

            // Gộp lại và lấy top 10 mới nhất
            var topActivities = activities
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            return new RecentActivityFeedResponse
            {
                Activities = topActivities
            };
        }
    }
}
