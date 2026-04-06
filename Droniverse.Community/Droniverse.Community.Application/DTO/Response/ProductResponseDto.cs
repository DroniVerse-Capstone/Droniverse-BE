using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public record ProductResponseDto(
        Guid ProductId,
        Guid ReferenceId,
        string ProductNameVN,
        string ProductNameEN,
        string DescriptionVN,
        string DescriptionEN,
        decimal Price,
        CurrencyType Currency,
        ProductStatus Status,
        DateTime CreateAt,
        DateTime UpdateAt
    )
    {
        public ProductResponseDto() : this(Guid.Empty, Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, 0, CurrencyType.USD, default, DateTime.MinValue, DateTime.MinValue)
        {
        }
    }
}
