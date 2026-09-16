using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Shared.Extensions;

public static class ControllerExtensions
{
    /// <summary>
    /// Helper method để upload image với error handling
    /// </summary>
    public static async Task<string> UploadImageAsync(
        this ControllerBase controller,
        ICloudinaryService cloudinaryService,
        FileUploadDto fileUploadDto,
        string folder)
    {
        if (fileUploadDto == null || fileUploadDto.File.Length == 0)
            return null;

        try
        {
            var url = await cloudinaryService.UploadImageAsync(fileUploadDto.File, folder);
            return url;
        }
        catch (ArgumentException ex)
        {
            return null;
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }
}

