using AutoMapper;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
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

            WithdrawRequest withdrawRequest = new WithdrawRequest(
                requesterId: userId,
                note: request.Note,
                amount: request.Amount,
                walletId: wallet.WalletID);

            WithdrawRequest createdWithdrawRequest = await _unitOfWork.WithdrawRequests.Add(withdrawRequest);
            wallet.UpdateBalance(-request.Amount);
            await _unitOfWork.Wallets.Update(wallet);

            Transaction transaction = new Transaction
            (
                walletId: wallet.WalletID,
                amount: (int)request.Amount,
                type: TransactionType.WITHDRAWAL,
                referenceID: createdWithdrawRequest.WithdrawRequestID // referenceID có thể là withdrawID
            );
            await _unitOfWork.Transactions.Add(transaction);

            await _unitOfWork.SaveChangeAsync();

            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy Người dùng với ID: " + userId);
            }

            WithdrawResponseDto response = new()
            {
                WithdrawID = createdWithdrawRequest.WithdrawRequestID,
                Amount = createdWithdrawRequest.Amount,
                Status = withdrawRequest.Status,
                CreatedAt = createdWithdrawRequest.CreatedAt,
                UpdatedAt = createdWithdrawRequest.UpdatedAt,
                ApprovedAt = createdWithdrawRequest.ApprovedAt,
                RequesterID = userId,
                ApproverID = createdWithdrawRequest.ApproverID,
                Note = request.Note,
                RejectReason = createdWithdrawRequest.RejectReason,
                Wallet = new WalletResponseDto
                {
                    WalletID = wallet.WalletID,
                    Bank = wallet.Bank,
                    BankNumber = wallet.BankNumber,
                    Balance = wallet.Balance,
                    OwnerID = wallet.OwnerID,
                    OwnerName = user.Username,
                    CreatedAt = wallet.CreatedAt,
                    UpdatedAt = wallet.UpdatedAt
                }
            };
            return response;
        }

        public async Task<WithdrawResponseDto> UpdateWithdrawRequestStatus(Guid withdrawRequestId, WithdrawApproveRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            WithdrawRequest? withdrawRequest = await _unitOfWork.WithdrawRequests.GetByCondition(w => w.WithdrawRequestID == withdrawRequestId);
            if (withdrawRequest == null)
                throw new NotFoundException("Không tìm thấy Yêu cầu rút tiền với ID: " + withdrawRequestId);


            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(withdrawRequest.RequesterID);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy Người dùng với ID: " + withdrawRequest.RequesterID);
            }

            // trừ tiền trong wallet
            Wallet? wallet = await _unitOfWork.Wallets.GetByCondition(w => w.OwnerID == user.UserId);
            if (wallet == null)
                throw new NotFoundException("Không tìm thấy Ví của Người dùng với ID: " + user.UserId);
            
            if(withdrawRequest.Status != WithdrawStatus.PENDING)
            {
                throw new InvalidOperationException("Chỉ có thể cập nhật trạng thái cho các yêu cầu đang ở trạng thái PENDING.");
            }
            if (request.Status == WithdrawStatus.APPROVED)
            {
                //cập nhật lại thành approved
                withdrawRequest.UpdateStatus(WithdrawStatus.APPROVED);
                withdrawRequest.ApproverID = _currentUserService.UserId;
                withdrawRequest.ApprovedAt = _clock.Now; // referenceID có thể là withdrawID

            }
            else if (request.Status == WithdrawStatus.REJECTED) // vi pham policy
            {
                wallet.UpdateBalance(withdrawRequest.Amount); // hoàn tiền vào ví
                withdrawRequest.RejectReason = request.RejectReason;
                withdrawRequest.UpdateStatus(WithdrawStatus.REJECTED);
                withdrawRequest.ApproverID = _currentUserService.UserId;
                withdrawRequest.ApprovedAt = _clock.Now;
                Transaction transaction = new Transaction
                (
                    walletId: wallet.WalletID,
                    amount: (int)withdrawRequest.Amount,
                    type: TransactionType.REFUND,
                    referenceID: withdrawRequest.WithdrawRequestID // referenceID có thể là withdrawID
                );
            }
            else if(request.Status == WithdrawStatus.CANCELED) //
            {
                wallet.UpdateBalance(withdrawRequest.Amount); // hoàn tiền vào ví
                withdrawRequest.UpdateStatus(WithdrawStatus.CANCELED);
                withdrawRequest.ApproverID = _currentUserService.UserId;
                withdrawRequest.ApprovedAt = _clock.Now;

                Transaction transaction = new Transaction
                (
                    walletId: wallet.WalletID,
                    amount: (int)withdrawRequest.Amount,
                    type: TransactionType.REFUND,
                    referenceID: withdrawRequest.WithdrawRequestID // referenceID có thể là withdrawID
                );
            }
            else
            {
                throw new ArgumentException("Trạng thái không hợp lệ.");
            }

            await _unitOfWork.SaveChangeAsync();

            WithdrawResponseDto response = new()
            {
                WithdrawID = withdrawRequest.WithdrawRequestID,
                Amount = withdrawRequest.Amount,
                Status = withdrawRequest.Status,
                CreatedAt = withdrawRequest.CreatedAt,
                UpdatedAt = withdrawRequest.UpdatedAt,
                ApprovedAt = withdrawRequest.ApprovedAt,
                RequesterID = user.UserId,
                ApproverID = withdrawRequest.ApproverID,
                Note = withdrawRequest.Note,
                RejectReason = withdrawRequest.RejectReason,
                Wallet = new WalletResponseDto
                {
                    WalletID = wallet.WalletID,
                    Bank = wallet.Bank,
                    BankNumber = wallet.BankNumber,
                    Balance = wallet.Balance,
                    OwnerID = wallet.OwnerID,
                    OwnerName = user.Username,
                    CreatedAt = wallet.CreatedAt,
                    UpdatedAt = wallet.UpdatedAt
                }
            };
            return response;
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
                throw new NotFoundException("Người dùng hiện tại chưa có ví.");
            }
            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(userId);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng hiện tại");

            WalletResponseDto response = _mapper.Map<WalletResponseDto>(wallet);
            response.OwnerName = user.Username;
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

        public async Task<IEnumerable<WithdrawResponseDto>> GetMyWithdrawRequestAsync()
        {
            Guid userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
            }

            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(userId);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng hiện tại");

            IEnumerable<WithdrawRequest> withdrawReqList = await _unitOfWork.WithdrawRequests.GetManyByCondition(w => w.RequesterID == user.UserId);

            if(withdrawReqList == null || !withdrawReqList.Any())
            {
                return new List<WithdrawResponseDto>();
            }

            Wallet? wallet = await _unitOfWork.Wallets.GetByCondition(w => w.OwnerID == user.UserId);
            if (wallet == null)
                throw new NotFoundException("Không tìm thấy Ví của Người dùng với ID: " + user.UserId);

            // Map từ IEnumerable<WithdrawRequest> sang IEnumerable<WithdrawResponseDto>
            var responses = withdrawReqList.Select(withdrawRequest => new WithdrawResponseDto
            {
                WithdrawID = withdrawRequest.WithdrawRequestID,
                Amount = withdrawRequest.Amount,
                Status = withdrawRequest.Status,
                CreatedAt = withdrawRequest.CreatedAt,
                UpdatedAt = withdrawRequest.UpdatedAt,
                ApprovedAt = withdrawRequest.ApprovedAt,
                RequesterID = user.UserId,
                ApproverID = withdrawRequest.ApproverID,
                Note = withdrawRequest.Note,
                RejectReason = withdrawRequest.RejectReason,
                Wallet = new WalletResponseDto
                {
                    WalletID = wallet.WalletID,
                    Bank = wallet.Bank,
                    BankNumber = wallet.BankNumber,
                    Balance = wallet.Balance,
                    OwnerID = wallet.OwnerID,
                    OwnerName = user.Username,
                    CreatedAt = wallet.CreatedAt,
                    UpdatedAt = wallet.UpdatedAt
                }
            }).ToList();

            return responses;
        }
    }
}
