using Microsoft.AspNetCore.Http;

namespace Droniverse.Shared.Services.IServices;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder = "droniverse");
    Task<bool> DeleteImageAsync(string publicId);
    Task<List<string>> UploadMultipleImagesAsync(IEnumerable<IFormFile> files, string folder = "droniverse");
}

