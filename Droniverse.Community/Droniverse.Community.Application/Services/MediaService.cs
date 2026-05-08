using AutoMapper;
using DnsClient.Internal;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Components.Sections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services;

public class MediaService : IMediaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly ILogger<MediaService> _logger;
    private readonly IClock _clock;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Droniverse.Shared.Services.IServices.ICloudinaryService _cloudinaryService;
    private const string MediaCacheKeyPrefix = "media";

    public MediaService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService,
        ILogger<MediaService> logger,
        IClock clock,
        IServiceScopeFactory serviceScopeFactory,
        Droniverse.Shared.Services.IServices.ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
        _logger = logger;
        _clock = clock;
        _serviceScopeFactory = serviceScopeFactory;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<IEnumerable<MediaResponseDto>> GetAllMedia()
    {
        try
        {
            var medias = await _unitOfWork.Medias.GetAll();
            return _mapper.Map<IEnumerable<MediaResponseDto>>(medias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all media");
            return Enumerable.Empty<MediaResponseDto>();
        }
    }

    public async Task<MediaResponseDto?> GetMediaById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Media ID cannot be empty.", nameof(id));

            var media = await _unitOfWork.Medias.GetByCondition(
                m => m.MediaID == id,
                q => q.Include(m => m.MediaType));

            if (media == null)
                return null;

            return _mapper.Map<MediaResponseDto>(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while getting media by ID: {id}");
            return null;
        }
    }

    public async Task<MediaResponseDto> UploadTempMedia(FileUploadMediaDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Upload request data cannot be null.");

        if (dto.File == null || dto.File.Length == 0)
            throw new ArgumentException("File is required and cannot be empty.", nameof(dto.File));

        // Convert enum to string
        string mediaTypeStr = dto.MediaType.ToString().ToUpper();

        // Get MediaType from database
        var mediaType = await _unitOfWork.MediaTypes
            .GetByCondition(mt => mt.TypeNameVN == mediaTypeStr);

        if (mediaType == null)
            throw new KeyNotFoundException($"MediaType with TypeNameVN '{mediaTypeStr}' not found in database.");

        // Upload file to Cloudinary
        string imageUrl = await _cloudinaryService.UploadMediaAsync(dto.File, mediaTypeStr, "droniverse/temporary");

        // Create Media entity
        var mediaId = Guid.NewGuid();
        var media = new Media
        {
            MediaID = mediaId,
            MediaTypeID = mediaType.MediaTypeID,
            Url = imageUrl,
            CreatedAt = _clock.Now,
            UpdatedAt = _clock.Now
        };

        // Fire-and-forget: Save to database in background without waiting
        _ = SaveMediaToDatabase(media);

        // Return response immediately after Cloudinary upload succeeds
        return new MediaResponseDto
        {
            MediaID = media.MediaID,
            MediaTypeID = media.MediaTypeID,
            MediaType = mediaType.TypeNameVN,
            Url = media.Url,
            CreatedAt = media.CreatedAt
        };
    }

    public async Task UploadMedia(Media media, string folder)
    {
        if (media == null)
            throw new ArgumentNullException(nameof(media));

        if (string.IsNullOrWhiteSpace(media.Url))
            throw new ArgumentException("Media.Url is empty.", nameof(media));

        try
        {
            // Load MediaType if not already loaded
            if (media.MediaType == null)
            {
                media = await _unitOfWork.Medias.GetByCondition(
                    m => m.MediaID == media.MediaID,
                    q => q.Include(m => m.MediaType));
                
                if (media == null)
                    throw new KeyNotFoundException("Media not found.");
            }

            var mediaTypeStr = media.MediaType?.TypeNameVN ?? "IMAGE";

            // Measure upload time for diagnostics
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Download current media from Cloudinary
            using var http = new System.Net.Http.HttpClient();
            var resp = await http.GetAsync(media.Url);
            resp.EnsureSuccessStatusCode();
            var fileBytes = await resp.Content.ReadAsByteArrayAsync();

            // Extract filename from URL
            var urlPath = new Uri(media.Url).AbsolutePath;
            var urlFileName = Path.GetFileName(urlPath);

            // Create FormFile object
            using var stream = new MemoryStream(fileBytes);
            var formFile = new Microsoft.AspNetCore.Http.FormFile(
                stream,
                0,
                stream.Length,
                "file",
                urlFileName);

            // Upload to new folder using UploadMediaAsync
            var newUrl = await _cloudinaryService.UploadMediaAsync(formFile, mediaTypeStr, folder);

            sw.Stop();
            _logger.LogInformation("Media {MediaId} successfully moved to folder {Folder} in {ms}ms", media.MediaID, folder, sw.ElapsedMilliseconds);

            // Update media record
            media.Url = newUrl;
            media.UpdatedAt = _clock.Now;

            await _unitOfWork.Medias.Update(media);
            await _unitOfWork.SaveChangeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while uploading/moving media {MediaId} to folder {Folder}", media.MediaID, folder);
            throw;
        }
    }

    /// <summary>
    /// Background task to save media to database without blocking the response
    /// Creates a new service scope to avoid DbContext disposal issues
    /// </summary>
    private async Task SaveMediaToDatabase(Media media)
    {
        try
        {
            // Create a new scope to avoid DbContext disposal issues
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.Medias.Add(media);
                await unitOfWork.SaveChangeAsync();
                _logger.LogInformation($"Media {media.MediaID} saved to database successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while saving media {media.MediaID} to database");
            // Don't throw - this is a background operation
        }
    }

    public async Task<IEnumerable<MediaMiniResponse>> GetMiniResponse(IEnumerable<Guid>? mediaIds)
    {
        try
        {
            var filteredMediaIds = (mediaIds ?? Enumerable.Empty<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (filteredMediaIds.Count == 0)
                return Enumerable.Empty<MediaMiniResponse>();

            var medias = await _unitOfWork.Medias.GetManyByCondition(m => filteredMediaIds.Contains(m.MediaID));
            return _mapper.Map<IEnumerable<MediaMiniResponse>>(medias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while get media data");
            return Enumerable.Empty<MediaMiniResponse>();
        }
    }

    public async Task DeleteMedia(Media media)
    {
        if (media == null)
            throw new ArgumentNullException(nameof(media));

        try
        {
            // Extract public_id from Cloudinary URL
            // URL format: https://res.cloudinary.com/{cloud_name}/image/upload/v{version}/{folder}/{public_id}.{extension}
            var publicId = ExtractPublicIdFromUrl(media.Url);
            
            if (!string.IsNullOrWhiteSpace(publicId))
            {
                // Delete from Cloudinary
                var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
                
                if (!deleteResult)
                {
                    _logger.LogWarning("Failed to delete image from Cloudinary: {PublicId}", publicId);
                }
                else
                {
                    _logger.LogInformation("Successfully deleted image from Cloudinary: {PublicId}", publicId);
                }
            }

            // Delete from database
            await _unitOfWork.Medias.Delete(media);
            await _unitOfWork.SaveChangeAsync();
            
            _logger.LogInformation("Media {MediaId} successfully deleted from database", media.MediaID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting media {MediaId}", media.MediaID);
            throw;
        }
    }

    private string ExtractPublicIdFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        try
        {
            // Cloudinary URL format: https://res.cloudinary.com/{cloud}/image/upload/v{version}/{folder}/{public_id}.{ext}
            var uri = new Uri(url);
            var path = uri.AbsolutePath; // /image/upload/v1234567890/folder/public_id.jpg
            
            // Get the last part and remove extension
            var lastPart = path.Split('/').LastOrDefault();
            if (string.IsNullOrEmpty(lastPart))
                return string.Empty;

            // Remove extension
            var publicId = System.IO.Path.GetFileNameWithoutExtension(lastPart);
            
            // Get full path: folder/public_id
            var segments = path.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
            {
                var folderStart = System.Array.IndexOf(segments, "upload");
                if (folderStart >= 0 && folderStart + 2 < segments.Length)
                {
                    // Reconstruct: everything after version is folder/public_id
                    var parts = segments.Skip(folderStart + 2).ToArray();
                    if (parts.Length > 0)
                    {
                        parts[parts.Length - 1] = System.IO.Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);
                        return string.Join("/", parts);
                    }
                }
            }

            return publicId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting public_id from URL: {Url}", url);
            return string.Empty;
        }
    }
}

