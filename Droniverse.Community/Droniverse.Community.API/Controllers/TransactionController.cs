using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    [ApiController]
    [Route("community/transactions")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ITransactionService _transactionService;
        private readonly ICurrentUserService _currentUserService;

        public TransactionController(
            IWalletService walletService,
            ITransactionService transactionService,
            ICurrentUserService currentUserService)
        {
            _walletService = walletService;
            _transactionService = transactionService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Lấy chi tiết giao dịch theo transaction ID
        /// </summary>
        [HttpGet("{transactionId}")]
        public async Task<ApiResponse> GetTransactionById(Guid transactionId)
        {
            TransactionResponseDto result = await _transactionService.GetTransactionByIdAsync(transactionId);
            return SuccessResponse<TransactionResponseDto>.Create(result, "Lấy chi tiết giao dịch thành công");
        }

        /// <summary>
        /// Lấy danh sách giao dịch của tôi (Club Manager)
        /// </summary>
        /// <param name="currentPage">Trang hiện tại (mặc định: 1)</param>
        /// <param name="pageSize">Số giao dịch trên một trang (mặc định: 5, tối đa: 20)</param>
        /// <param name="type">Loại giao dịch (COMMISSION, WITHDRAWAL, REFUND) - tùy chọn</param>
        [HttpGet("me")]
        public async Task<ApiResponse> GetMyTransactions(
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] TransactionType? type = null)
        {
            var request = new TransactionSearchRequest
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                Type = type,
                SortDirection = Droniverse.Shared.Enums.SortDirection.Desc
            };

            PaginationResult<IEnumerable<TransactionResponseDto>> result = await _transactionService.GetMyTransactionsAsync(request);
            return SuccessResponse<PaginationResult<IEnumerable<TransactionResponseDto>>>.Create(result, "Lấy danh sách giao dịch của tôi thành công");
        }

        /// <summary>
        /// Lấy danh sách tất cả giao dịch (có phân trang và filter theo loại)
        /// </summary>
        /// <param name="currentPage">Trang hiện tại (mặc định: 1)</param>
        /// <param name="pageSize">Số giao dịch trên một trang (mặc định: 5, tối đa: 20)</param>
        /// <param name="type">Loại giao dịch (COMMISSION, WITHDRAWAL, REFUND) - tùy chọn</param>
        /// <param name="createdFrom">Ngày tạo từ (tùy chọn)</param>
        /// <param name="createdTo">Ngày tạo đến (tùy chọn)</param>
        /// <param name="sortDirection">Hướng sắp xếp (Asc/Desc, mặc định: Desc)</param>
        [HttpGet]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetAllTransactions(
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] TransactionType? type = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] Droniverse.Shared.Enums.SortDirection sortDirection = Droniverse.Shared.Enums.SortDirection.Desc)
        {
            var request = new TransactionSearchRequest
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                Type = type,
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                SortDirection = sortDirection
            };

            PaginationResult<IEnumerable<TransactionResponseDto>> result = await _transactionService.GetAllTransactionsAsync(request);
            return SuccessResponse<PaginationResult<IEnumerable<TransactionResponseDto>>>.Create(result, "Lấy danh sách giao dịch thành công");
        }

        /// <summary>
        /// Tạo giao dịch mới
        /// </summary>
        //[HttpPost]
        //[Authorize(Roles = Roles.AdminOrSystemManager)]
        //public async Task<ApiResponse> CreateTransaction([FromBody] CreateTransactionDto request)
        //{
        //    TransactionResponseDto result = await _transactionService.CreateTransactionAsync(
        //        request.WalletId,
        //        request.Amount,
        //        request.Type,
        //        request.ReferenceId
        //    );
        //    return SuccessResponse<TransactionResponseDto>.Create(result, "Tạo giao dịch thành công");
        //}
    }
}