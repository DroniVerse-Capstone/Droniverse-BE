using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Request
{
    public record ProductCategoryRequestDto(
        string Code,
        string CategoryNameVN,
        string CategoryNameEN,
        string DescriptionVN,
        string DescriptionEN
    )
    {
        public ProductCategoryRequestDto() : this(default,default,default, default, default)
        {
        }
    }
}
