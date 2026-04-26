using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            IUnitOfWork unitOfWork,
            IClock clock, 
            IMapper mapper,
            IdentityMicroserviceClient identityMicroserviceClient,
            ICurrentUserService currentUserService,
            ILogger<TransactionService> logger)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _identityMicroserviceClient = identityMicroserviceClient;
            _logger = logger;
        }

        public async Task<TransactionResponseDto> GetTransactionByIdAsync(Guid transactionId)
        {
            Transaction? transaction = await _unitOfWork.Transactions.GetByCondition(
                t => t.TransactionID == transactionId,
                include: q => q.Include(t => t.Wallet)
                              .Include(t => t.WithdrawRequest));
            if (transaction == null)
            {
                throw new NotFoundException($"Transaction with ID {transactionId} not found");
            }

            TransactionResponseDto response = _mapper.Map<TransactionResponseDto>(transaction);
            await PopulateOwnerNameAsync(response);
            return response;
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetTransactionsByWalletIdAsync(Guid walletId)
        {
            // Verify wallet exists
            Wallet? wallet = await _unitOfWork.Wallets.GetByCondition(w => w.WalletID == walletId);
            if (wallet == null)
            {
                throw new NotFoundException($"Wallet with ID {walletId} not found");
            }

            IEnumerable<Transaction> transactions = await _unitOfWork.Transactions.GetTransactionsByWalletIdAsync(walletId);
            IEnumerable<TransactionResponseDto> response = _mapper.Map<IEnumerable<TransactionResponseDto>>(transactions);
            
            foreach (var transaction in response)
            {
                await PopulateOwnerNameAsync(transaction);
            }
            
            return response;
        }

        public async Task<PaginationResult<IEnumerable<TransactionResponseDto>>> GetAllTransactionsAsync(TransactionSearchRequest request)
        {
            IEnumerable<Transaction> allTransactions = await _unitOfWork.Transactions.GetManyByCondition(
                t => true,
                include: q => q.Include(t => t.Wallet));

            // Apply filters
            if (request.Type.HasValue)
            {
                allTransactions = allTransactions.Where(t => t.Type == request.Type.Value);
            }

            if (request.CreatedFrom.HasValue)
            {
                allTransactions = allTransactions.Where(t => t.CreatedAt >= request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                allTransactions = allTransactions.Where(t => t.CreatedAt <= request.CreatedTo.Value);
            }

            // Apply sorting
            if (request.SortDirection == SortDirection.Desc)
            {
                allTransactions = allTransactions.OrderByDescending(t => t.CreatedAt);
            }
            else
            {
                allTransactions = allTransactions.OrderBy(t => t.CreatedAt);
            }

            // Get total count before pagination
            int totalRecords = allTransactions.Count();

            // Apply pagination
            var pagedTransactions = allTransactions
                .Skip((request.CurrentPage - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            IEnumerable<TransactionResponseDto> mappedTransactions = _mapper.Map<IEnumerable<TransactionResponseDto>>(pagedTransactions);

            foreach (var transaction in mappedTransactions)
            {
                await PopulateOwnerNameAsync(transaction);
            }

            return new PaginationResult<IEnumerable<TransactionResponseDto>>(
                mappedTransactions,
                totalRecords,
                request.CurrentPage,
                request.PageSize
            );
        }

        public async Task<PaginationResult<IEnumerable<TransactionResponseDto>>> GetMyTransactionsAsync(TransactionSearchRequest request)
        {
            // Get current user's wallet
            Guid currentUserId = _currentUserService.UserId;
            Wallet? userWallet = await _unitOfWork.Wallets.GetByCondition(w => w.OwnerID == currentUserId);
            
            if (userWallet == null)
            {
                throw new NotFoundException($"Wallet not found for user {currentUserId}");
            }

            // Get transactions for the user's wallet
            IEnumerable<Transaction> allTransactions = await _unitOfWork.Transactions.GetManyByCondition(
                t => t.WalletID == userWallet.WalletID,
                include: q => q.Include(t => t.Wallet));

            // Apply filters
            if (request.Type.HasValue)
            {
                allTransactions = allTransactions.Where(t => t.Type == request.Type.Value);
            }

            // Apply sorting
            if (request.SortDirection == SortDirection.Desc)
            {
                allTransactions = allTransactions.OrderByDescending(t => t.CreatedAt);
            }
            else
            {
                allTransactions = allTransactions.OrderBy(t => t.CreatedAt);
            }

            // Get total count before pagination
            int totalRecords = allTransactions.Count();

            // Apply pagination
            var pagedTransactions = allTransactions
                .Skip((request.CurrentPage - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            IEnumerable<TransactionResponseDto> mappedTransactions = _mapper.Map<IEnumerable<TransactionResponseDto>>(pagedTransactions);

            foreach (var transaction in mappedTransactions)
            {
                await PopulateOwnerNameAsync(transaction);
            }

            return new PaginationResult<IEnumerable<TransactionResponseDto>>(
                mappedTransactions,
                totalRecords,
                request.CurrentPage,
                request.PageSize
            );
        }

        private async Task PopulateOwnerNameAsync(TransactionResponseDto response)
        {
            if (response?.Wallet == null)
            {
                return;
            }

            try
            {
                var user = await _identityMicroserviceClient.GetUserByUserID(response.Wallet.OwnerID);
                response.Wallet.OwnerName = user?.Username ?? "Người dùng";
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to fetch user name from Identity service for OwnerID: {response.Wallet.OwnerID}. Error: {ex.Message}");
                response.Wallet.OwnerName = "Người dùng";
            }
        }
    }
}
