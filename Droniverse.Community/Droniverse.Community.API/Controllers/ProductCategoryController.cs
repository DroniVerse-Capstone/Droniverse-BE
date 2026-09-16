using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.IService;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/product-category")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly ILogger<ProductCategoryController> _logger;
        private readonly IProductCategoryService _proCateService;

        public ProductCategoryController(
            ILogger<ProductCategoryController> logger,
            IProductCategoryService productCategoryService)
        {
            _logger = logger;
            _proCateService = productCategoryService;
        }

        //[HttpGet]

        //public async Task<IActionResult> GetProducts()
        //{
        //    var products = await _productService.GetAllProducts();
        //    return Ok(products);

        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var productCate = await _proCateService.GetProductCategoryById(id);
            if (productCate == null)
            {
                return NotFound();
            }
            return Ok(productCate);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCategoryRequestDto productRequest)
        {
            var createdProCate = await _proCateService.CreateProductCategory(productRequest);
            return CreatedAtAction(nameof(GetById), new { id = createdProCate.CategoryId }, createdProCate);
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductRequestDto productRequest)
        //{
        //    var updatedProduct = await _productService.UpdateProduct(id, productRequest);
        //    if (updatedProduct == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(updatedProduct);
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteProduct(Guid id)
        //{
        //    var isDeleted = await _productService.Delete(id);
        //    if (!isDeleted)
        //    {
        //        return NotFound();
        //    }
        //    return NoContent();
        //}
    } 
}
