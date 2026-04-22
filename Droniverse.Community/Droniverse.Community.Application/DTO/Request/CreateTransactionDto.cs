using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Request;

public record CreateTransactionDto
{
    public Guid WalletId { get; set; }
    public int Amount { get; set; }
    public TransactionType Type { get; set; }
    public Guid ReferenceId { get; set; }
}
