using Droniverse.Community.Application.DTO.Response;
namespace Droniverse.Community.Application.IService;

public interface IWalletService
{
    Task<WalletResponseDto> CreateWallet(WalletCreateRequestDto request);
    Task<WalletResponseDto> GetWalletById(Guid walletId);
}

