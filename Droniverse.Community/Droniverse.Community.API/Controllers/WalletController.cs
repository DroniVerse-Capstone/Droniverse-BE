using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Controllers
{
    [ApiController]
    [Route("community/wallets")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }
        
        /// <summary>
        /// Lấy ra danh sách các request gửi yêu cầu rút tiền của tôi (Club manager)
        /// </summary>
        /// <returns></returns>
        [HttpGet("withdraw-request/me")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<WithdrawResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> GetMyWithdrawRequest()
        {
            IEnumerable<WithdrawResponseDto> response = await _walletService.GetMyWithdrawRequestAsync();
            return SuccessResponse<IEnumerable<WithdrawResponseDto>>.Create(response, "Lấy danh sách gửi yêu cầu rút tiền của club manager hiện tại thành công.");
        }

        /// <summary>
        /// Lấy tất cả yêu cầu rút tiền (Admin/System Manager), có filter theo status và thời gian tạo
        /// </summary>
        [HttpGet("withdraw-request")]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<WithdrawResponseDto>>>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> GetAllWithdrawRequests(
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] WithdrawStatus? status = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] SortDirection sortDirection = SortDirection.Desc)
        {
            var request = new WithdrawSearchRequest
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                Status = status,
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                SortDirection = sortDirection
            };

            var result = await _walletService.GetAllWithdrawRequestsAsync(request);
            return SuccessResponse<PaginationResult<IEnumerable<WithdrawResponseDto>>>.Create(result, "Lấy danh sách yêu cầu rút tiền thành công.");
        }


        /// <summary>
        /// Club manager tạo yêu cầu rút tiền
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("withdraw-request")]
        [ProducesResponseType(typeof(SuccessResponse<WithdrawResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> CreateWithdrawRequest([FromBody] WithdrawRequestDto request)
        {
            WithdrawResponseDto response =  await _walletService.CreateWithdrawRequest(request);
            return SuccessResponse<WithdrawResponseDto>.Create(response, "Yêu cầu rút tiền đã được gửi thành công");
        }


        /// <summary>
        /// Lấy thông tin ví của club manager đang đăng nhập
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        [Authorize(Roles = Roles.ClubManager)]
        [ProducesResponseType(typeof(SuccessResponse<WalletResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> GetMyWallet()
        {
            WalletResponseDto result = await _walletService.GetMyWallet();

            return SuccessResponse<WalletResponseDto>.Create(result, "Lấy thông tin ví thành công");
        }

        /// <summary>
        /// Lấy thông tin của ví theo walletID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        [ProducesResponseType(typeof(SuccessResponse<WalletResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> GetById(Guid id)
        {
            WalletResponseDto result = await _walletService.GetWalletById(id);

            return SuccessResponse<WalletResponseDto>.Create(result, "Lấy thông tin ví thành công");
        }

        /// <summary>
        /// Tạo mới ví cho club manager
        /// </summary>
        /// <remarks>
        /// API trả về thông tin ví đã được tạo, bao gồm WalletID, OwnerID, OwnerName, BankNumber, Bank, Balance, CreatedAt và UpdatedAt. Các trường này cung cấp thông tin chi tiết về ví mới được tạo ra cho club manager.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về thông tin ví đã được tạo
        /// </returns>
        [HttpPost]
        [Authorize(Roles = Roles.ClubManager)]
        [SwaggerRequestExample(typeof(WalletRequestDto), typeof(WalletRequestExample))]
        [ProducesResponseType(typeof(SuccessResponse<WalletResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> Create([FromBody] WalletRequestDto request)
        {
            WalletResponseDto result = await _walletService.CreateWallet(request);
            return SuccessResponse<WalletResponseDto>.Create(result, "Tạo ví thành công");
        }

        /// <summary>
        /// Cập nhật thông tin của ví cho club manager, gồm các thông tin như Bank (tên ngân hàng), BankNumber (số tài khoản ngân hàng)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.ClubManager)]
        [SwaggerRequestExample(typeof(WalletRequestDto), typeof(WalletRequestExample))]
        [ProducesResponseType(typeof(SuccessResponse<WalletResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> UpdateWalletInfo(Guid id, [FromBody] WalletRequestDto request)
        {
            WalletResponseDto result = await _walletService.UpdateWallet(request);
            return SuccessResponse<WalletResponseDto>.Create(result, "Cập nhật ví thành công");

        }

        [HttpPut("withdraw-request/{id}/status")]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        [SwaggerRequestExample(typeof(WithdrawApproveRequestDto), typeof(WithdrawApproveRequestExample))]
        [ProducesResponseType(typeof(SuccessResponse<WithdrawResponseDto>), StatusCodes.Status200OK)]
        public async Task<ApiResponse> UpdateWithdrawRequestStatus(Guid id, [FromBody] WithdrawApproveRequestDto request)
        {
            WithdrawResponseDto result = await _walletService.UpdateWithdrawRequestStatus(id, request);
            return SuccessResponse<WithdrawResponseDto>.Create(result, "Cập nhật trạng thái yêu cầu rút tiền thành công");
        }
    }
}