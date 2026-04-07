using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Droniverse.Shared.Services.IServices;
using Droniverse.Shared.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;

namespace Droniverse.Shared.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    public CloudinaryService(IOptions<CloudinarySettings> settings)
    {
        _settings = settings.Value;

        var account = new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret
            );

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true; // Sử dụng HTTPS cho tất cả các URL
    }
    public async Task<bool> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        };

        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder = "droniverse")
    {
        if(file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty.", nameof(file));

        //Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");

        // Validate file size (10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            throw new ArgumentException("File size exceeds 10MB limit");
        }

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto"), // Auto optimize format
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<List<string>> UploadMultipleImagesAsync(IEnumerable<IFormFile> files, string folder = "droniverse")
    {
        var urls = new List<string>();

        foreach (var file in files)
        {
            var url = await UploadImageAsync(file, folder);
            urls.Add(url);
        }

        return urls;
    }
}

