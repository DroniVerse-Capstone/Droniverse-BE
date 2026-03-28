using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request;

public record ProductRequestDto
(
    [Required]
        [Length(1,255, ErrorMessage = "Product name must be from 1 to 255 characters !!")]
        string ProductNameVN,
    [Required]
        [Length(1,255, ErrorMessage = "Product type name must be from 1 to 255 characters !!")]
        string ProductNameEN,
    string DescriptionVN,
    string DescriptionEN,
    Guid ReferenceId,
    Guid CategoryId,
    decimal Price,
    CurrencyType Currency,
    ProductStatus Status
)
{
    public ProductRequestDto() : this(string.Empty, string.Empty, string.Empty, string.Empty, Guid.Empty, Guid.Empty, 0, CurrencyType.USD, default)
    {
    }
}
