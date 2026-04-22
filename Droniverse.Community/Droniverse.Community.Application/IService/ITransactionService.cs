using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.IService;

public interface ITransactionService
{
    Task<TransactionResponseDto> GetTransactionByIdAsync(Guid transactionId);
    Task<IEnumerable<TransactionResponseDto>> GetTransactionsByWalletIdAsync(Guid walletId);
    Task<PaginationResult<IEnumerable<TransactionResponseDto>>> GetMyTransactionsAsync(TransactionSearchRequest request);
    Task<PaginationResult<IEnumerable<TransactionResponseDto>>> GetAllTransactionsAsync(TransactionSearchRequest request);
    Task<TransactionResponseDto> CreateTransactionAsync(Guid walletId, int amount, TransactionType type, Guid referenceId);
}

