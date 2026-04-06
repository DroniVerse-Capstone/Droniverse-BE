using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/test")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly CommunityMicroserviceClient _communityMicroserviceClient;

    public TestController(CommunityMicroserviceClient communityMicroserviceClient)
    {
        _communityMicroserviceClient = communityMicroserviceClient;
    }

    [HttpGet("community/product-by-reference/{referenceId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse<ProductMiniResponseDTO?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductByReferenceId(Guid referenceId, CancellationToken cancellationToken)
    {
        var product = await _communityMicroserviceClient.GetProductByReferenceIdAsync(referenceId, cancellationToken);
        return Ok(SuccessResponse<ProductMiniResponseDTO?>.Create(product, "Test get product by reference id thành công."));
    }

    [HttpPost("community/products-by-references")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ProductMiniResponseDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByReferenceIds([FromBody] IEnumerable<Guid> referenceIds, CancellationToken cancellationToken)
    {
        var products = await _communityMicroserviceClient.GetProductsBulkByReferenceIdsAsync(referenceIds, cancellationToken);
        return Ok(SuccessResponse<IEnumerable<ProductMiniResponseDTO>>.Create(products, "Test get products by reference ids thành công."));
    }
}
