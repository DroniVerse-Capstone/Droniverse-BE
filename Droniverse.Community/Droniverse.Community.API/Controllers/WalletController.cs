using Droniverse.Community.API.Examples;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
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

        [HttpGet("withdraw-request/me")]
        public async Task<ApiResponse> GetMyWithdrawRequest()
        {
            IEnumerable<WithdrawResponseDto> response = await _walletService.GetMyWithdrawRequestAsync();
            return SuccessResponse<IEnumerable<WithdrawResponseDto>>.Create(response, "Lấy danh sách gửi yêu cầu rút tiền của club manager hiện tại thành công.");
        }


        /// <summary>
        /// Club manager tạo yêu cầu rút tiền
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("withdraw-request")]
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
        [Authorize(Roles = Roles.SystemRoles)]
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
        public async Task<ApiResponse> UpdateWalletInfo(Guid id, [FromBody] WalletRequestDto request)
        {
            WalletResponseDto result = await _walletService.UpdateWallet(request);
            return SuccessResponse<WalletResponseDto>.Create(result, "Cập nhật ví thành công");

        }

        [HttpPut("withdraw-request/{id}/status")]
        [Authorize(Roles = Roles.AdminOrManagerRoles)]
        [SwaggerRequestExample(typeof(WithdrawApproveRequestDto), typeof(WithdrawApproveRequestExample))]
        public async Task<ApiResponse> UpdateWithdrawRequestStatus(Guid id, [FromBody] WithdrawApproveRequestDto request)
        {
            WithdrawResponseDto result = await _walletService.UpdateWithdrawRequestStatus(id, request);
            return SuccessResponse<WithdrawResponseDto>.Create(result, "Cập nhật trạng thái yêu cầu rút tiền thành công");
        }
    }
}