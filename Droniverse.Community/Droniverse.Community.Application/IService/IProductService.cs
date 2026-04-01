using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Community.Application.IService;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllProducts();
    Task<ProductResponseDto> GetProductById(Guid id);
    Task<ProductResponseDto> CreateProduct(ProductRequestDto request);
    Task<ProductResponseDto> UpdateProduct(Guid productID, ProductRequestDto request);
    Task<bool> Delete(Guid id);
}

