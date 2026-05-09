using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.IService
{
    public interface IDetailDashboardService
    {
        Task<PaginationResult<IEnumerable<DetailDashboardUserResponse>>> GetClubMembersWithCourseSpend(int page = 1, int pageSize = 10);
        Task<PaginationResult<IEnumerable<DetailDashboardClubManagerResponse>>> GetClubManagersWithWalletBalance(int page = 1, int pageSize = 10);
        Task<IEnumerable<UserOrderDetailResponseDto>> GetUserOrderDetails(Guid userId);
        Task<IEnumerable<TransactionResponseDto>> GetUserTransactions(Guid userId);
        Task<IEnumerable<CourseStatisticInterServiceDto>> GetCourseDashboard();
        Task<IEnumerable<CourseDetailDashboardResponseDto>> GetCourseRevenueDashboard();
    }
}
