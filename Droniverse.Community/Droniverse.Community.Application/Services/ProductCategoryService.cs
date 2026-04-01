using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductCategoryService> _logger;
    public ProductCategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductCategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductCategoryResponseDto> CreateProductCategory(ProductCategoryRequestDto request)
    {
        ProductCategory productCategory = _mapper.Map<ProductCategory>(request);

        ProductCategory addedProductCate = await _unitOfWork.ProductCategories.Add(productCategory);
        await _unitOfWork.SaveChangeAsync();
        ProductCategoryResponseDto response = _mapper.Map<ProductCategoryResponseDto>(addedProductCate);

        return response;
    }

    public async Task<bool> Delete(Guid id)
    {
        Product? product = await _unitOfWork.Products.GetByCondition(p => p.ProductID == id);
        if(product == null)
        {
            return false;
        }
        await _unitOfWork.Products.Delete(product);
        return true;
    }

    public async Task<IEnumerable<ProductCategoryResponseDto>> GetAllProductCategories()
    {
        IEnumerable<Product> products = await _unitOfWork.Products.GetAll();
        return _mapper.Map<IEnumerable<ProductCategoryResponseDto>>(products);
    }

    public async Task<ProductCategoryResponseDto> GetProductCategoryById(Guid id)
    {
        ProductCategory? product = await _unitOfWork.ProductCategories.GetByCondition(p => p.CategoryID == id);
        return _mapper.Map<ProductCategoryResponseDto>(product);
    }

    public async Task<ProductCategoryResponseDto> UpdateProductCategory(Guid productID, ProductCategoryRequestDto request)
    {
        Product? product = await _unitOfWork.Products.GetByCondition(p => p.ProductID == productID);
        if (product == null)
        {
            throw new NotFoundException($"Product with id #{productID} not found");
        }

        Product? updatedProduct = await _unitOfWork.Products.Update(product);
        return _mapper.Map<ProductCategoryResponseDto>(updatedProduct);


    }
}

