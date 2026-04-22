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

            var (productIds, courseIdByProductId) = await GetClubProductContext(clubId);
            if (productIds.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var allOrders = await _orderRepository.GetAllSuccessfulOrders();
            if (allOrders == null)
                allOrders = [];

            // Filter for CLUB_IMPORT orders for this club (club's spending/expense)
            var clubImportOrders = allOrders
                .Where(o => o.ClubID == clubId && o.OrderType == Domain.Enums.OrderType.CLUB_IMPORT)
                .ToList();

            if (clubImportOrders.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var expenseByCourseId = clubImportOrders
                .Where(x => x.Item?.ProductID != null && courseIdByProductId.ContainsKey(x.Item.ProductID))
                .GroupBy(x => courseIdByProductId[x.Item.ProductID])
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

            if (expenseByCourseId.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var courseIds = expenseByCourseId.Keys.ToList();
            var academyCourses = await _academyMicroserviceClient.GetCoursesByIdsSimple(clubId);
            var filteredAcademyCourses = academyCourses
                .Where(c => courseIds.Contains(c.CourseId))
                .ToList();

            var courseById = filteredAcademyCourses
                .GroupBy(c => c.CourseId)
                .ToDictionary(g => g.Key, g => g.First());

            var stats = expenseByCourseId
                .Select(kvp =>
                {
                    courseById.TryGetValue(kvp.Key, out var course);
                    return new CourseRevenueStat
                    {
                        CourseInfo = course ?? throw new Exception("Lỗi hệ thống"),
                        Revenue = kvp.Value
                    };
                })
                .OrderByDescending(x => x.Revenue)
                .Take(top)
                .ToList();

            return new ClubCourseRevenueResponse { RevenueByCourse = stats };
        }

        private async Task<(List<Guid> ProductIds, Dictionary<Guid, Guid> CourseIdByProductId)> GetClubProductContext(Guid clubId)
        {
           throw new NotImplementedException("Chưa implement mapping ProductID -> CourseID. Cần có thêm thông tin về Product để thực hiện mapping này.");
        }

        private static double CalculateGrowthRate(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0)
                return currentValue > 0 ? 100 : 0;

            return Math.Round((double)((currentValue - previousValue) / previousValue * 100), 2);
        }

        public async Task<AdminRevenueOverviewResponse> GetAdminRevenueOverview()
        {
            IEnumerable<Club> clubList = await _unitOfWork.Clubs.GetAll();

            if (clubList == null || !clubList.Any())
                return new AdminRevenueOverviewResponse
                {
                    TotalRevenue = 0,
                    RevenueThisMonth = 0,
                    RevenueLastMonth = 0,
                    RevenueGrowthRate = 0,
                    NetProfit = 0,
                    ProfitThisMonth = 0,
                    ProfitLastMonth = 0,
                    ProfitGrowthRate = 0,
                    TotalTransactions = 0,
                    TransactionsThisMonth = 0
                };

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

            // Calculate Revenue and Profit from CLUB_IMPORT orders (clubs' spending on course codes)
            var clubImportOrders = allOrdersList.Where(o => o.OrderType == Domain.Enums.OrderType.CLUB_IMPORT).ToList();
            var totalRevenue = clubImportOrders.Sum(o => o.TotalAmount);
            var revenueThisMonth = clubImportOrders
                .Where(o => o.Payment != null 
                    && o.Payment.TransactionDate >= startThisMonth 
                    && o.Payment.TransactionDate < startNextMonth)
                .Sum(o => o.TotalAmount);
            var revenueLastMonth = clubImportOrders
                .Where(o => o.Payment != null 
                    && o.Payment.TransactionDate >= startLastMonth 
                    && o.Payment.TransactionDate < startThisMonth)
                .Sum(o => o.TotalAmount);

            // Profit is also based on CLUB_IMPORT orders
            var netProfit = clubImportOrders.Sum(o => o.TotalAmount);
            var profitThisMonth = clubImportOrders
                .Where(o => o.Payment != null 
                    && o.Payment.TransactionDate >= startThisMonth 
                    && o.Payment.TransactionDate < startNextMonth)
                .Sum(o => o.TotalAmount);
            var profitLastMonth = clubImportOrders
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

            var clubList = await _unitOfWork.Clubs.GetAll();
            if (clubList == null || !clubList.Any())
                return new RevenueGrowthResponse { RevenueGrowth = [] };

            var now = _clock.Now;
            var startCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var fromMonth = startCurrentMonth.AddMonths(-(months - 1));
            var toExclusive = startCurrentMonth.AddMonths(1);

            var allRevenueData = new List<OrderRevenueData>();

            // Aggregate revenue data from all clubs using CLUB_IMPORT orders (admin's revenue)
            foreach (var club in clubList)
            {
                try
                {
                    var revenueData = await _orderRepository.GetSuccessfulRevenueDataByClubId(club.ClubID, OrderType.CLUB_IMPORT, fromMonth, toExclusive);
                    allRevenueData.AddRange(revenueData);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other clubs
                    continue;
                }
            }

            if (allRevenueData.Count == 0)
                return new RevenueGrowthResponse { RevenueGrowth = [] };

            var valueByMonth = allRevenueData
                .GroupBy(x => new DateTime(x.PaidAt.Year, x.PaidAt.Month, 1))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Revenue));

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

            var clubList = await _unitOfWork.Clubs.GetAll();
            if (clubList == null || !clubList.Any())
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var allRevenueData = new List<OrderRevenueData>();
            var allCourseIdByProductId = new Dictionary<Guid, Guid>();

            // Aggregate revenue data and product context from all clubs using CLUB_IMPORT orders (admin's revenue)
            foreach (var club in clubList)
            {
                try
                {
                    var revenueData = await _orderRepository.GetSuccessfulRevenueDataByClubId(club.ClubID, OrderType.CLUB_IMPORT);
                    allRevenueData.AddRange(revenueData);

                    var (productIds, courseIdByProductId) = await GetClubProductContext(club.ClubID);
                    foreach (var kvp in courseIdByProductId)
                    {
                        if (!allCourseIdByProductId.ContainsKey(kvp.Key))
                            allCourseIdByProductId[kvp.Key] = kvp.Value;
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with other clubs
                    continue;
                }
            }

            if (allRevenueData.Count == 0 || allCourseIdByProductId.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var revenueByCourseId = allRevenueData
                .Where(x => allCourseIdByProductId.ContainsKey(x.ProductId))
                .GroupBy(x => allCourseIdByProductId[x.ProductId])
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Revenue));

            if (revenueByCourseId.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var allCourses = new List<SimpleCourseResponse>();
            foreach (var club in clubList)
            {
                var coursesByClub = await _academyMicroserviceClient.GetCoursesByIdsSimple(club.ClubID);
                allCourses.AddRange(coursesByClub);
            }

            var courseById = allCourses
                .GroupBy(c => c.CourseId)
                .ToDictionary(g => g.Key, g => g.First());

            var stats = revenueByCourseId
                .Select(kvp =>
                {
                    courseById.TryGetValue(kvp.Key, out var course);
                    return new CourseRevenueStat
                    {
                        CourseInfo = course ?? throw new Exception("Lỗi hệ thống"),
                        Revenue = kvp.Value
                    };
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

            // Filter CLUB_IMPORT orders
            var clubImportOrders = allOrders
                .Where(o => o.OrderType == Domain.Enums.OrderType.CLUB_IMPORT)
                .ToList();

            if (clubImportOrders.Count == 0)
                return new AdminClubRankingResponse { Clubs = [] };

            // Group CLUB_IMPORT orders by ClubID
            var ordersByClub = clubImportOrders
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
