using System.ComponentModel.DataAnnotations;
using Droniverse.Community.Domain.Enums;

public record WalletRequestDto
{
    [Required]
    public string BankNumber { get; init; }

    [Required]
    public string Bank { get; init; }

}