using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;
using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.DTO.Response;

public record WithdrawResponseDto
{
    public Guid WithdrawID { get; set; }
    public decimal Amount { get; set; }
    public WithdrawStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid RequesterID { get; set; }
    public Guid? ApproverID { get; set; }
    public string? Note { get; set; }
    public string? RejectReason { get; set; }
    public WalletResponseDto? Wallet { get; set; }
}