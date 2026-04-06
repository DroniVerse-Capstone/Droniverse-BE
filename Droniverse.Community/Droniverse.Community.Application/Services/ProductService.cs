using AutoMapper;
using DnsClient.Internal;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Microsoft.AspNetCore.Components.Sections;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;
    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductResponseDto> CreateProduct(ProductRequestDto request)
    {

        Product product = _mapper.Map<Product>(request);
        Product addedProduct = await _unitOfWork.Products.Add(product);
        //_logger.LogInformation($"ID của product: {addedProduct.ProductID}");
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

    public async Task<ProductResponseDto> UpdateProduct(Guid productID, ProductRequestDto request)
    {
        Product? product = await _unitOfWork.Products.GetByCondition(p => p.ProductID == productID);
        if (product == null)
        {
            throw new NotFoundException($"Product with id #{productID} not found");
        }
        Product? updatedProduct = await _unitOfWork.Products.Update(product);
        return _mapper.Map<ProductResponseDto>(updatedProduct);


    }
}

