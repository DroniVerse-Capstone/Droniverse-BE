
using Droniverse.Community.Domain.Enums;
using System.ComponentModel.DataAnnotations;

public record WithdrawApproveRequestDto
{
    [Required]
    public WithdrawStatus Status { get; set; }

    public string? RejectReason { get; set; }
}