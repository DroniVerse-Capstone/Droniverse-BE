using AutoMapper;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Community.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly ICurrentUserService _currentUserService;

        public WalletService(
            IUnitOfWork unitOfWork,
            IOrderRepository orderRepository,
            AcademyMicroserviceClient academyMicroserviceClient,
            IClock clock, IMapper mapper,
            IdentityMicroserviceClient identityMicroserviceClient,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _academyMicroserviceClient = academyMicroserviceClient;
            _clock = clock;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _identityMicroserviceClient = identityMicroserviceClient;
        }

        public async Task<WalletResponseDto> CreateWallet(WalletRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (await CheckDuplicateWalletInfo(request))
            {
                throw new InvalidOperationException("Thông tin ví đã tồn tại.");
            }

            Guid userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
            }
            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy Người dùng với ID: " + userId);
            }

            Wallet? existingWallet = await _unitOfWork.Wallets.GetByCondition(w => w.OwnerID == userId);
            if (existingWallet != null)
            {
                throw new InvalidOperationException("Người dùng đã có ví.");
            }

            Wallet wallet = new Wallet(userId, request.Bank, request.BankNumber);
            Wallet createdWallet = await _unitOfWork.Wallets.Add(wallet);
            await _unitOfWork.SaveChangeAsync();


            WalletResponseDto response = _mapper.Map<WalletResponseDto>(createdWallet);
            response.OwnerName = user.Username;

            return response;
        }

        public async Task<WithdrawResponseDto> CreateWithdrawRequest(WithdrawRequestDto request)
        {
            Guid userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
            }

            Wallet? wallet = await _unitOfWork.Wallets.GetByCondition(w => w.OwnerID == userId);
            if (wallet == null)
            {
                throw new NotFoundException("Người dùng hiện tại chưa có ví.");
            }

            if (request.Amount == null || request.Amount <= 0)
            {
                throw new ArgumentException("Số tiền rút phải lớn hơn 0.");
            }

            if (wallet.Balance < request.Amount)
            {
                throw new InvalidOperationException("Số dư trong ví không đủ để thực hiện rút tiền.");
            }
            return new WithdrawResponseDto { };

        }

        public async Task<WalletResponseDto> GetMyWallet()
        {
            Guid userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
            }

            Wallet? wallet = await _unitOfWork.Wallets.GetByCondition(w => w.OwnerID == userId);
            if (wallet == null)
            {
                throw new Exception("Người dùng hiện tại chưa có ví.");
            }
            WalletResponseDto response = _mapper.Map<WalletResponseDto>(wallet);

            return response;
        }

        public async Task<WalletResponseDto> GetWalletById(Guid walletId)
        {
            Wallet? wallet = await _unitOfWork.Wallets.GetByCondition(w => w.WalletID == walletId);
            if (wallet == null)
            {
                throw new NotFoundException("Không tìm thấy Ví với ID: " + walletId);
            }

            WalletResponseDto response = _mapper.Map<WalletResponseDto>(wallet);
            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(wallet.OwnerID);
            if (user != null)
            {
                response.OwnerName = user.Username;
            }
            return response;
        }

        public async Task<WalletResponseDto> UpdateWallet(WalletRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (await CheckDuplicateWalletInfo(request))
            {
                throw new InvalidOperationException("Thông tin ví đã tồn tại.");
            }

            Guid userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
            }
            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy Người dùng với ID: " + userId);
            }

            Wallet? wallet = await _unitOfWork.Wallets.GetByCondition(w => w.OwnerID == userId);
            if (wallet == null)
            {
                throw new NotFoundException("Không tìm thấy Ví của Người dùng với ID: " + userId);
            }
            wallet.Bank = request.Bank;
            wallet.BankNumber = request.BankNumber;
            Wallet? updatedWallet = await _unitOfWork.Wallets.Update(wallet);
            await _unitOfWork.SaveChangeAsync();
            WalletResponseDto response = _mapper.Map<WalletResponseDto>(updatedWallet);
            response.OwnerName = user.Username;
            return response;
        }

        //Nếu bị trùng tài khoản => trả về true, ngược lại trả về false
        private async Task<bool> CheckDuplicateWalletInfo(WalletRequestDto request)
        {
            Wallet? existingWallet = await _unitOfWork.Wallets.GetByCondition(w => w.Bank == request.Bank && w.BankNumber == request.BankNumber);
            return existingWallet != null;
        }
    }
}
