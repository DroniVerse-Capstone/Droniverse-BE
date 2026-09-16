using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Extensions;
using Droniverse.Shared.Services;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.API.Controllers;

[Route("identity/upload")]
[ApiController]
public class UploadController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;
    public UploadController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("temp")]
    public async Task<string> UploadAvatar([FromForm] FileUploadDto file)
    {
        return await this.UploadImageAsync(_cloudinaryService, file, "droniverse/temp");
    }
}

