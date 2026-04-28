using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response;

public record TransactionResponseDto
{
    public Guid TransactionID { get; set; }
    public WalletResponseDto Wallet { get; set; }
    public int Amount { get; set; }
    public TransactionType Type { get; set; }
    public Guid ClubID { get; set; }
    public Guid ReferenceID { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderResponseDto? Order { get; set; }
    public WithdrawResponseDto? WithdrawRequest { get; set; }

}