using Droniverse.Community.Application.IService;
using Droniverse.Academy.Application.Enums;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.Services;
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
            var (productIds, _) = await GetClubProductContext(clubId);
            if (productIds.Count == 0)
                return new RevenueOverviewResponse();

            var revenueData = (await _orderRepository.GetSuccessfulRevenueDataByProductIds(productIds)).ToList();
            if (revenueData.Count == 0)
                return new RevenueOverviewResponse();

            var now = _clock.Now;
            var startThisMonth = new DateTime(now.Year, now.Month, 1);
            var startNextMonth = startThisMonth.AddMonths(1);
            var startLastMonth = startThisMonth.AddMonths(-1);

            var totalRevenue = revenueData.Sum(x => x.Revenue);
            var revenueThisMonth = revenueData
                .Where(x => x.PaidAt >= startThisMonth && x.PaidAt < startNextMonth)
                .Sum(x => x.Revenue);
            var revenueLastMonth = revenueData
                .Where(x => x.PaidAt >= startLastMonth && x.PaidAt < startThisMonth)
                .Sum(x => x.Revenue);

            var growthRate = revenueLastMonth == 0
                ? (revenueThisMonth > 0 ? 100 : 0)
                : (double)((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100);

            return new RevenueOverviewResponse
            {
                TotalRevenue = totalRevenue,
                RevenueThisMonth = revenueThisMonth,
                RevenueLastMonth = revenueLastMonth,
                GrowthRate = Math.Round(growthRate, 2)
            };
        }

        public async Task<RevenueGrowthResponse> GetRevenueGrowthByClub(Guid clubId, int months)
        {
            if (months <= 0)
                months = 12;

            var (productIds, _) = await GetClubProductContext(clubId);
            var now = _clock.Now;
            var startCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var fromMonth = startCurrentMonth.AddMonths(-(months - 1));
            var toExclusive = startCurrentMonth.AddMonths(1);

            if (productIds.Count == 0)
            {
                return new RevenueGrowthResponse
                {
                    RevenueGrowth = Enumerable.Range(0, months)
                        .Select(i => fromMonth.AddMonths(i))
                        .Select(m => new MonthlyStat { Month = m.ToString("yyyy-MM"), Value = 0 })
                        .ToList()
                };
            }

            var revenueData = await _orderRepository.GetSuccessfulRevenueDataByProductIds(productIds, fromMonth, toExclusive);

            var valueByMonth = revenueData
                .GroupBy(x => new DateTime(x.PaidAt.Year, x.PaidAt.Month, 1))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Revenue));

            var growth = Enumerable.Range(0, months)
                .Select(i => fromMonth.AddMonths(i))
                .Select(m => new MonthlyStat
                {
                    Month = m.ToString("yyyy-MM"),
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

            var revenueData = await _orderRepository.GetSuccessfulRevenueDataByProductIds(productIds);

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
    }
}
