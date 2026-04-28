using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
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
        private readonly IClock _clock;

        public DashboardService(
            IUnitOfWork unitOfWork,
            IOrderRepository orderRepository,
            AcademyMicroserviceClient academyMicroserviceClient,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _academyMicroserviceClient = academyMicroserviceClient;
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

            // Get all successful orders across all clubs
            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null)
                allOrders = [];
            
            var allOrdersList = allOrders.ToList();

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
                TransactionsThisMonth = transactionsThisMonth
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
    }
}
