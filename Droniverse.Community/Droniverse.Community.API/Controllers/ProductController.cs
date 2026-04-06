using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;

        public ProductController(
            ILogger<ProductController> logger,
            IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }

        [HttpGet]

        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetAllProducts();
            return Ok(products);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("reference/{referenceId}")]
        public async Task<ActionResult<ProductMiniResponseDto>> GetProductByReferenceId(Guid referenceId)
        {
            var product = await _productService.GetProductByReferenceId(referenceId);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost("reference/bulk")]
        public async Task<ActionResult<IEnumerable<ProductMiniResponseDto>>> GetProductsBulkByReferenceIds([FromBody] IEnumerable<Guid> referenceIds)
        {
            var products = await _productService.GetProductsBulkByReferenceIds(referenceIds);
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductRequestDto productRequest)
        {
            var createdProduct = await _productService.CreateProduct(productRequest);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.ProductId }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductRequestDto productRequest)
        {
            var updatedProduct = await _productService.UpdateProduct(id, productRequest);
            if (updatedProduct == null)
            {
                return NotFound();
            }
            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var isDeleted = await _productService.Delete(id);
            if (!isDeleted)
            {
                return NotFound();
            }
            return NoContent();
        }
    } 
}
