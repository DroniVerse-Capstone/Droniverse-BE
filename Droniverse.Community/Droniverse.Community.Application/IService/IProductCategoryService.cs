using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Community.Application.IService;

public interface IProductCategoryService
{
    Task<IEnumerable<ProductCategoryResponseDto>> GetAllProductCategories();
    Task<ProductCategoryResponseDto> GetProductCategoryById(Guid id);
    Task<ProductCategoryResponseDto> CreateProductCategory(ProductCategoryRequestDto request);
    Task<ProductCategoryResponseDto> UpdateProductCategory(Guid ProductCategoryID, ProductCategoryRequestDto request);
    Task<bool> Delete(Guid id);
}

