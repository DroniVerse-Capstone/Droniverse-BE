using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Response;

public record WalletResponseDto
{
    public Guid WalletID { get; set; }
    public Guid OwnerID { get; set; }
    public string OwnerName { get; set; }
    public string BankNumber { get; set; }
    public string Bank { get; set; }
    public decimal Balance { get; set; }
     public DateTime CreatedAt { get; set; }
     public DateTime UpdatedAt { get; set; }

}