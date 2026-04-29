using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Droniverse.Community.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            IUnitOfWork unitOfWork,
            IClock clock, 
            IMapper mapper,
            IOrderRepository orderRepository,
            IdentityMicroserviceClient identityMicroserviceClient,
            ICurrentUserService currentUserService,
            ILogger<TransactionService> logger)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
            _mapper = mapper;
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
            _identityMicroserviceClient = identityMicroserviceClient;
            _logger = logger;
        }

        public async Task<TransactionResponseDto> GetTransactionByIdAsync(Guid transactionId)
        {
            Transaction? transaction = await _unitOfWork.Transactions.GetByCondition(
                t => t.TransactionID == transactionId,
                include: q => q.Include(t => t.Wallet)
                              .Include(t => t.Club)
                              .Include(t => t.WithdrawRequest));
            if (transaction == null)
            {
                throw new NotFoundException($"Transaction with ID {transactionId} not found");
            }

            TransactionResponseDto response = _mapper.Map<TransactionResponseDto>(transaction);
            await EnrichTransactionResponseAsync(transaction, response);
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
            
            foreach (var (entity, dto) in transactions.Zip(response, (entity, dto) => (entity, dto)))
            {
                await EnrichTransactionResponseAsync(entity, dto);
                await PopulateOwnerNameAsync(dto);
            }
            
            return response;
        }

        public async Task<PaginationResult<IEnumerable<TransactionResponseDto>>> GetAllTransactionsAsync(TransactionSearchRequest request)
        {
            IEnumerable<Transaction> allTransactions = await _unitOfWork.Transactions.GetManyByCondition(
                t => true,
                include: q => q.Include(t => t.Wallet)
                              .Include(t => t.Club));

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
            if (request.SortDirection == Shared.Enums.SortDirection.Desc)
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

            foreach (var (entity, dto) in pagedTransactions.Zip(mappedTransactions, (entity, dto) => (entity, dto)))
            {
                await EnrichTransactionResponseAsync(entity, dto);
                await PopulateOwnerNameAsync(dto);
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

            Participation? participation = await _unitOfWork.Participations.GetByCondition(p => p.ApproverID == currentUserId);
            if (participation == null)
            {
                throw new NotFoundException($"Participation not found for user {currentUserId}");
            }
            Guid clubID = participation.ClubID;

            // Get transactions for the user's wallet
            IEnumerable<Transaction> allTransactions = await _unitOfWork.Transactions.GetManyByCondition(
                t => t.WalletID == userWallet.WalletID,
                include: q => q.Include(t => t.Wallet)
                              .Include(t => t.Club));

            // Apply filters
            if (request.Type.HasValue)
            {
                allTransactions = allTransactions.Where(t => t.Type == request.Type.Value);
            }

            // Apply sorting
            if (request.SortDirection == Shared.Enums.SortDirection.Desc)
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

            foreach (var (entity, dto) in pagedTransactions.Zip(mappedTransactions, (entity, dto) => (entity, dto)))
            {
                await EnrichTransactionResponseAsync(entity, dto);
                if (dto.Club == null)
                {
                    dto.Club = new ClubMiniResponse { ClubID = clubID, NameVN = string.Empty, NameEN = string.Empty, ImageUrl = string.Empty };
                }
                else
                {
                    dto.Club.ClubID = clubID;
                }
                await PopulateOwnerNameAsync(dto);
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

        private async Task EnrichTransactionResponseAsync(Transaction transaction, TransactionResponseDto response)
        {
            if (transaction.Type == TransactionType.COMMISSION)
            {
                response.Order = await GetOrderResponseAsync(transaction.OrderID ?? transaction.ReferenceID);
                response.WithdrawRequest = null;
                return;
            }

            if (transaction.Type == TransactionType.WITHDRAWAL || transaction.Type == TransactionType.REFUND)
            {
                response.Order = null;
                response.WithdrawRequest = await GetWithdrawRequestResponseAsync(transaction.WithdrawRequestID ?? transaction.ReferenceID);
                return;
            }

            response.Order = null;
            response.WithdrawRequest = null;
        }

        private async Task<OrderResponseDto?> GetOrderResponseAsync(Guid orderId)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(order => order._id, orderId);
            Order? order = await _orderRepository.GetOrderByCondition(filter);
            if (order == null)
            {
                return null;
            }

            OrderResponseDto? orderResponse = _mapper.Map<OrderResponseDto?>(order);
            if (orderResponse == null)
            {
                return null;
            }

            UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(order.UserID);
            if (user != null)
            {
                return orderResponse with { User = user };
            }

            return orderResponse with
            {
                User = new UserResponse(
                    order.UserID,
                    order.UserName,
                    string.Empty,
                    string.Empty,
                    order.UserEmail,
                    null,
                    string.Empty,
                    null,
                    default,
                    null,
                    [],
                    [])
            };
        }

        private async Task<WithdrawResponseDto?> GetWithdrawRequestResponseAsync(Guid withdrawRequestId)
        {
            WithdrawRequest? withdrawRequest = await _unitOfWork.WithdrawRequests.GetByCondition(
                w => w.WithdrawRequestID == withdrawRequestId,
                include: q => q.Include(w => w.Wallet));

            if (withdrawRequest == null)
            {
                return null;
            }

            Wallet? wallet = withdrawRequest.Wallet;
            UserResponse? user = wallet == null
                ? null
                : await _identityMicroserviceClient.GetUserByUserID(wallet.OwnerID);

            return new WithdrawResponseDto
            {
                WithdrawID = withdrawRequest.WithdrawRequestID,
                Amount = withdrawRequest.Amount,
                Status = withdrawRequest.Status,
                CreatedAt = withdrawRequest.CreatedAt,
                UpdatedAt = withdrawRequest.UpdatedAt,
                ApprovedAt = withdrawRequest.ApprovedAt,
                RequesterID = withdrawRequest.RequesterID,
                ApproverID = withdrawRequest.ApproverID,
                Note = withdrawRequest.Note,
                RejectReason = withdrawRequest.RejectReason,
                Wallet = wallet == null
                    ? null
                    : new WalletResponseDto
                    {
                        WalletID = wallet.WalletID,
                        Bank = wallet.Bank,
                        BankNumber = wallet.BankNumber,
                        Balance = wallet.Balance,
                        OwnerID = wallet.OwnerID,
                        OwnerName = user?.Username ?? string.Empty,
                        CreatedAt = wallet.CreatedAt,
                        UpdatedAt = wallet.UpdatedAt
                    }
            };
        }
    }
}
