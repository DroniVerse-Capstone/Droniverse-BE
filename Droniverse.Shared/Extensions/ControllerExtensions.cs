using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Shared.Extensions;

public static class ControllerExtensions
{
    /// <summary>
    /// Helper method để upload image với error handling
    /// </summary>
    public static async Task<IActionResult> UploadImageAsync(
        this ControllerBase controller,
        ICloudinaryService cloudinaryService,
        FileUploadDto fileUploadDto,
        string folder)
    {
        if (fileUploadDto == null || fileUploadDto.File.Length == 0)
            return controller.BadRequest(new { message = "No file uploaded" });

        try
        {
            var url = await cloudinaryService.UploadImageAsync(fileUploadDto.File, folder);
            return controller.Ok(new { url });
        }
        catch (ArgumentException ex)
        {
            return controller.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, new { message = "Upload failed" });
        }
    }
}

