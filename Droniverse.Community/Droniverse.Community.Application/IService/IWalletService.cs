using Droniverse.Community.Application.DTO.Response;
namespace Droniverse.Community.Application.IService;

public interface IWalletService
{
    Task<WalletResponseDto> CreateWallet(WalletRequestDto request);
    Task<WalletResponseDto> UpdateWallet(WalletRequestDto request);
    Task<WalletResponseDto> GetWalletById(Guid walletId);
    Task<WalletRequestDto> GetMyWallet();
}

