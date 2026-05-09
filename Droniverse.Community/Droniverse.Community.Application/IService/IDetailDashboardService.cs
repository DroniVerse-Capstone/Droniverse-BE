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
        Task<PaginationResult<IEnumerable<UserOrderDetailResponseDto>>> GetUserOrderDetails(Guid userId, int page = 1, int pageSize = 10);
        Task<PaginationResult<IEnumerable<TransactionResponseDto>>> GetUserTransactions(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<CourseStatisticInterServiceDto>> GetCourseDashboard();
        Task<PaginationResult<IEnumerable<CourseDetailDashboardResponseDto>>> GetCourseRevenueDashboard(int page = 1, int pageSize = 10);
        Task<PaginationResult<IEnumerable<CourseRevenueByClubResponseDto>>> GetCourseRevenueByClub(Guid courseId, int page = 1, int pageSize = 10);
        Task<PaginationResult<IEnumerable<ClubDashboardResponseDto>>> GetClubDashboardAsync(int page = 1, int pageSize = 10);
        Task<PaginationResult<IEnumerable<ClubMemberTransactionResponseDto>>> GetClubMemberTransactionsAsync(Guid clubId, Guid? courseId, int page = 1, int pageSize = 10);
    }
}
