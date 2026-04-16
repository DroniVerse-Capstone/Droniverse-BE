using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
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

            var netProfit = aggregate.TotalRevenue - aggregate.TotalExpense;
            var profitThisMonth = aggregate.RevenueThisMonth - aggregate.ExpenseThisMonth;
            var profitLastMonth = aggregate.RevenueLastMonth - aggregate.ExpenseLastMonth;

            var revenueGrowthRate = CalculateGrowthRate(aggregate.RevenueThisMonth, aggregate.RevenueLastMonth);
            var profitGrowthRate = CalculateGrowthRate(profitThisMonth, profitLastMonth);

            return new RevenueOverviewResponse
            {
                TotalRevenue = aggregate.TotalRevenue,
                RevenueThisMonth = aggregate.RevenueThisMonth,
                RevenueLastMonth = aggregate.RevenueLastMonth,
                RevenueGrowthRate = revenueGrowthRate,

                TotalExpense = aggregate.TotalExpense,
                ExpenseThisMonth = aggregate.ExpenseThisMonth,
                ExpenseLastMonth = aggregate.ExpenseLastMonth,

                NetProfit = netProfit,
                ProfitThisMonth = profitThisMonth,
                ProfitLastMonth = profitLastMonth,
                ProfitGrowthRate = profitGrowthRate,

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

            var revenueData = await _orderRepository.GetSuccessfulRevenueDataByClubId(clubId, fromMonth, toExclusive);

            var valueByMonth = revenueData
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

            return new RevenueGrowthResponse { RevenueGrowth = growth };
        }

        public async Task<ClubCourseRevenueResponse> GetRevenueByCourseByClub(Guid clubId, int top)
        {
            if (top <= 0)
                top = 10;

            var (productIds, courseIdByProductId) = await GetClubProductContext(clubId);
            if (productIds.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var revenueData = await _orderRepository.GetSuccessfulRevenueDataByClubId(clubId);

            var revenueByCourseId = revenueData
                .Where(x => courseIdByProductId.ContainsKey(x.ProductId))
                .GroupBy(x => courseIdByProductId[x.ProductId])
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Revenue));

            if (revenueByCourseId.Count == 0)
                return new ClubCourseRevenueResponse { RevenueByCourse = [] };

            var courseIds = revenueByCourseId.Keys.ToList();
            var academyCourses = await _academyMicroserviceClient.GetCoursesByIdsSimple(courseIds);

            var courseById = academyCourses
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

        private async Task<(List<Guid> ProductIds, Dictionary<Guid, Guid> CourseIdByProductId)> GetClubProductContext(Guid clubId)
        {
            var clubExists = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId, q => q.AsNoTracking());
            if (clubExists == null)
                throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

            var clubCourseIds = (await _unitOfWork.ClubCourses.GetManyByCondition(
                cc => cc.ClubID == clubId,
                q => q.AsNoTracking()))
                .Select(cc => cc.CourseID)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (clubCourseIds.Count == 0)
                return ([], []);

            var products = (await _unitOfWork.Products.GetManyByCondition(
                p => clubCourseIds.Contains(p.ReferenceID),
                q => q.AsNoTracking())).ToList();

            if (products.Count == 0)
                return ([], []);

            var courseIdByProductId = products
                .GroupBy(p => p.ProductID)
                .ToDictionary(g => g.Key, g => g.First().ReferenceID);

            return (courseIdByProductId.Keys.ToList(), courseIdByProductId);
        }

        private static double CalculateGrowthRate(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0)
                return currentValue > 0 ? 100 : 0;

            return Math.Round((double)((currentValue - previousValue) / previousValue * 100), 2);
        }

        public async Task<RevenueOverviewResponse> GetAdminRevenueOverview()
        {
            IEnumerable<Club> clubList = await _unitOfWork.Clubs.GetAll();

            if (clubList == null || !clubList.Any())
                return new RevenueOverviewResponse
                {
                    TotalRevenue = 0,
                    RevenueThisMonth = 0,
                    RevenueLastMonth = 0,
                    RevenueGrowthRate = 0,
                    TotalExpense = 0,
                    ExpenseThisMonth = 0,
                    ExpenseLastMonth = 0,
                    NetProfit = 0,
                    ProfitThisMonth = 0,
                    ProfitLastMonth = 0,
                    ProfitGrowthRate = 0,
                    TotalTransactions = 0,
                    TransactionsThisMonth = 0
                };

            // Aggregate all club data
            decimal totalRevenue = 0;
            decimal revenueThisMonth = 0;
            decimal revenueLastMonth = 0;
            decimal totalExpense = 0;
            decimal expenseThisMonth = 0;
            decimal expenseLastMonth = 0;
            int totalTransactions = 0;
            int transactionsThisMonth = 0;

            foreach (var club in clubList)
            {
                try
                {
                    var clubRevenue = await GetRevenueOverviewByClub(club.ClubID);

                    totalRevenue += clubRevenue.TotalRevenue;
                    revenueThisMonth += clubRevenue.RevenueThisMonth;
                    revenueLastMonth += clubRevenue.RevenueLastMonth;
                    totalExpense += clubRevenue.TotalExpense;
                    expenseThisMonth += clubRevenue.ExpenseThisMonth;
                    expenseLastMonth += clubRevenue.ExpenseLastMonth;
                    totalTransactions += clubRevenue.TotalTransactions;
                    transactionsThisMonth += clubRevenue.TransactionsThisMonth;
                }
                catch (Exception ex)
                {
                    // Log error but continue with other clubs
                    continue;
                }
            }

            var netProfit = totalRevenue - totalExpense;
            var profitThisMonth = revenueThisMonth - expenseThisMonth;
            var profitLastMonth = revenueLastMonth - expenseLastMonth;

            var revenueGrowthRate = CalculateGrowthRate(revenueThisMonth, revenueLastMonth);
            var profitGrowthRate = CalculateGrowthRate(profitThisMonth, profitLastMonth);

            return new RevenueOverviewResponse
            {
                TotalRevenue = totalRevenue,
                RevenueThisMonth = revenueThisMonth,
                RevenueLastMonth = revenueLastMonth,
                RevenueGrowthRate = revenueGrowthRate,

                TotalExpense = totalExpense,
                ExpenseThisMonth = expenseThisMonth,
                ExpenseLastMonth = expenseLastMonth,

                NetProfit = netProfit,
                ProfitThisMonth = profitThisMonth,
                ProfitLastMonth = profitLastMonth,
                ProfitGrowthRate = profitGrowthRate,

                TotalTransactions = totalTransactions,
                TransactionsThisMonth = transactionsThisMonth
            };
        }
    }
}
