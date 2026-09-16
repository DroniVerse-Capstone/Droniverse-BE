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

        // Validate file size (20MB)
        if (file.Length > 20 * 1024 * 1024)
        {
            throw new ArgumentException("File size exceeds 20MB limit");
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

    public async Task<string> UploadTempImageAsync(IFormFile file, string folder = "droniverse")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty.", nameof(file));

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".ico" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException($"Invalid file type '{fileExtension}' for file '{file.FileName}'. Allowed: {string.Join(", ", allowedExtensions)}");

        if (file.Length > 20 * 1024 * 1024)
            throw new ArgumentException("File size exceeds 20MB limit");

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<string> UploadImageAsync(byte[] content, string fileName, string contentType, string folder = "droniverse")
    {
        if (content == null || content.Length == 0)
            throw new ArgumentException("File content is null or empty.", nameof(content));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");

        if (content.Length > 20 * 1024 * 1024)
            throw new ArgumentException("File size exceeds 20MB limit");

        await using var stream = new MemoryStream(content);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto"),
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");

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

    public async Task<string> UploadMediaAsync(IFormFile file, string mediaType, string folder = "droniverse")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty.", nameof(file));

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var mediaTypeUpper = mediaType.ToUpper();

        // Determine resource type and validate extensions based on media type
        ResourceType resourceType;
        string[] allowedExtensions;
        long maxFileSize;

        switch (mediaTypeUpper)
        {
            case "VIDEO":
                resourceType = ResourceType.Video;
                allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv", ".webm", ".m4v", ".3gp" };
                maxFileSize = 100 * 1024 * 1024; // 100MB
                break;

            case "IMAGE":
                resourceType = ResourceType.Image;
                allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".ico" };
                maxFileSize = 10 * 1024 * 1024; // 10MB
                break;

            case "GIF":
                resourceType = ResourceType.Video; // GIF is treated as video in Cloudinary
                allowedExtensions = new[] { ".gif" };
                maxFileSize = 50 * 1024 * 1024; // 50MB for GIF
                break;

            case "AUDIO":
                resourceType = ResourceType.Video; // Audio is uploaded as raw resource
                allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac", ".m4a", ".aac", ".wma" };
                maxFileSize = 50 * 1024 * 1024; // 50MB for audio
                break;

            default:
                throw new ArgumentException($"Invalid media type. Allowed: IMAGE, VIDEO, GIF, AUDIO");
        }

        // Validate file extension
        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException($"Invalid file type for {mediaTypeUpper}. Allowed: {string.Join(", ", allowedExtensions)}");

        // Validate file size
        if (file.Length > maxFileSize)
            throw new ArgumentException($"File size exceeds {maxFileSize / (1024 * 1024)}MB limit for {mediaTypeUpper}");

        await using var stream = file.OpenReadStream();

        dynamic uploadResult;

        if (mediaTypeUpper == "VIDEO" || mediaTypeUpper == "GIF")
        {
            var videoUploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
            uploadResult = await _cloudinary.UploadAsync(videoUploadParams);
        }
        else if (mediaTypeUpper == "IMAGE")
        {
            var imageUploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto"),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
            uploadResult = await _cloudinary.UploadAsync(imageUploadParams);
        }
        else // AUDIO
        {
            var rawUploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
            uploadResult = await _cloudinary.UploadAsync(rawUploadParams);
        }

        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }
}

