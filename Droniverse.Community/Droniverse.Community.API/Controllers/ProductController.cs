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

        /// <summary>
        /// Lấy ra danh sách các sản phẩm
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]

        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetAllProducts();
            return Ok(products);

        }

        /// <summary>
        /// Lấy ra tt chi tiết của sản phẩm theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        /// <summary>
        /// Lấy thông tin của sản phẩm theo referenceId(courseId, droneId,...)
        /// </summary>
        /// <param name="referenceId"></param>
        /// <returns></returns>
        [HttpGet("reference/{referenceId}")]
        [ProducesResponseType(typeof(ProductMiniResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Tạo sản phẩm
        /// </summary>
        /// <param name="productRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductRequestDto productRequest)
        {
            var createdProduct = await _productService.CreateProduct(productRequest);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.ProductId }, createdProduct);
        }

        /// <summary>
        /// Cập nhật sản phẩm
        /// </summary>
        /// <param name="id"></param>
        /// <param name="productRequest"></param>
        /// <returns></returns>

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
