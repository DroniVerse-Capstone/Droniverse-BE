using Droniverse.Community.Application.DTO.Response;
namespace Droniverse.Community.Application.IService;

public interface IWalletService
{
    Task<WithdrawResponseDto> CreateWithdrawRequest(WithdrawRequestDto request);
    Task<WalletResponseDto> CreateWallet(WalletRequestDto request);
    Task<WalletResponseDto> UpdateWallet(WalletRequestDto request);
    Task<WalletResponseDto> GetWalletById(Guid walletId);
    Task<WalletResponseDto> GetMyWallet();
    Task<WithdrawResponseDto> UpdateWithdrawRequestStatus(Guid withdrawRequestId, WithdrawApproveRequestDto request);
    Task<IEnumerable<WithdrawResponseDto>> GetMyWithdrawRequestAsync();
}

