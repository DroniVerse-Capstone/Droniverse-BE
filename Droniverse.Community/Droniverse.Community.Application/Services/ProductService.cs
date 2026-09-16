using AutoMapper;
using DnsClient.Internal;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Components.Sections;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductService> _logger;
    private const string ProductCacheKeyPrefix = "product";
    private readonly IClock _clock;
    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService,
        ILogger<ProductService> logger,
        IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
        _logger = logger;
        _clock = clock;
    }

    public async Task<ProductResponseDto> CreateProduct(ProductRequestDto request)
    {

        Product product = _mapper.Map<Product>(request);
        ProductCategory? category = await _unitOfWork.ProductCategories.GetByCondition(c => c.CategoryID == Guid.Parse("16993e60-b569-4a76-9d3a-3f0fdc0da64b"));
        product.ProductCategory = category;
        Product addedProduct = await _unitOfWork.Products.Add(product);
        await _unitOfWork.SaveChangeAsync();
        ProductResponseDto response = _mapper.Map<ProductResponseDto>(addedProduct);

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

    public async Task<IEnumerable<ProductResponseDto>> GetAllProducts()
    {
        IEnumerable<Product> products = await _unitOfWork.Products.GetAll();
        return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
    }

    public async Task<ProductResponseDto> GetProductById(Guid id)
    {
        Product? product = await _unitOfWork.Products.GetByCondition(p => p.ProductID == id);
        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<ProductMiniResponseDto?> GetProductByReferenceId(Guid referenceId)
    {
        if (referenceId == Guid.Empty)
            return null;

        Product? product = await _unitOfWork.Products.GetByCondition(p => p.ReferenceID == referenceId);
        if (product == null)
            return null;

        return _mapper.Map<ProductMiniResponseDto>(product);
    }

    public async Task<IEnumerable<ProductMiniResponseDto>> GetProductsBulkByReferenceIds(IEnumerable<Guid> referenceIds)
    {
        if (referenceIds == null)
            return [];

        var distinctReferenceIds = referenceIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctReferenceIds.Any())
            return [];

        IEnumerable<Product> products = await _unitOfWork.Products
            .GetManyByCondition(p => distinctReferenceIds.Contains(p.ReferenceID));

        return _mapper.Map<IEnumerable<ProductMiniResponseDto>>(products);
    }

    public async Task<ProductResponseDto> UpdateProduct(Guid productID, ProductRequestDto request)
    {
        Product? product = await _unitOfWork.Products.GetByCondition(p => p.ProductID == productID);
        if (product == null)
        {
            throw new NotFoundException($"Product with id #{productID} not found");
        }

        // Cập nhật tất cả fields từ request
        product.ProductNameVN = request.ProductNameVN;
        product.ProductNameEN = request.ProductNameEN;
        product.DescriptionVN = request.DescriptionVN;
        product.DescriptionEN = request.DescriptionEN;
        product.ReferenceID = request.ReferenceId;
        product.Price = request.Price;
        product.Currency = request.Currency;
        product.Status = request.Status;
        product.UpdateAt = _clock.Now;

        // Set ProductCategory
        ProductCategory? category = await _unitOfWork.ProductCategories.GetByCondition(c => c.CategoryID == Guid.Parse("16993e60-b569-4a76-9d3a-3f0fdc0da64b"));
        product.ProductCategory = category;

        Product? updatedProduct = await _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangeAsync();
        var cacheKey = BuildProductCacheKey(productID);
        var cachedProduct = await _cacheService.GetAsync<ProductResponseDto>(cacheKey);
        if (cachedProduct != null)
        {
            await _cacheService.RemoveAsync(cacheKey);
        }

        return _mapper.Map<ProductResponseDto>(updatedProduct);
    }

    private static string BuildProductCacheKey(Guid productId)
    {
        return $"{ProductCacheKeyPrefix}:{productId}";
    }
}

