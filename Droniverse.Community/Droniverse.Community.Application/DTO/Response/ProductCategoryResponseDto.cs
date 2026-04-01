using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public record ProductCategoryResponseDto(
        Guid CategoryId,
        //ICollection<ProductResponseDto> Products,
        string Code,
        string CategoryNameVN,
        string CategoryNameEN,
        string DescriptionVN,
        string DescriptionEN,
        DateTime CreateAt,
        DateTime UpdateAt
    )
    {
        public ProductCategoryResponseDto() : this(default, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, default,default)
        {
        }
    }
}
