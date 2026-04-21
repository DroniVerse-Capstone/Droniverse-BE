using System.ComponentModel.DataAnnotations;
using Droniverse.Community.Domain.Enums;

public record WalletCreateRequestDto
{
    [Required]
    public Guid OwnerId { get; set; }

    [Required]
    public string BankNumber { get; init; }

    [Required]
    public string Bank { get; init; }

}