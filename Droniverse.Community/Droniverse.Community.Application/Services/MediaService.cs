using AutoMapper;
using DnsClient.Internal;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Components.Sections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.Services;

public class MediaService : IMediaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly ILogger<MediaService> _logger;
    private readonly IClock _clock;
    private const string MediaCacheKeyPrefix = "media";

    public MediaService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService,
        ILogger<MediaService> logger,
        IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
        _logger = logger;
        _clock = clock;
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
            return [];
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

    public async Task<MediaResponseDto> UploadTempMedia(FileUploadMediaDto dto, ICloudinaryService cloudinaryService)
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
        string imageUrl = await cloudinaryService.UploadMediaAsync(dto.File, mediaTypeStr, "droniverse/temp");

        // Create Media entity
        var mediaId = Guid.NewGuid();
        var media = new Media
        {
            MediaID = mediaId,
            MediaTypeID = mediaType.MediaTypeID,
            ImageUrl = imageUrl,
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
            ImageUrl = media.ImageUrl,
            CreatedAt = media.CreatedAt
        };
    }

    /// <summary>
    /// Background task to save media to database without blocking the response
    /// </summary>
    private async Task SaveMediaToDatabase(Media media)
    {
        try
        {
            await _unitOfWork.Medias.Add(media);
            await _unitOfWork.SaveChangeAsync();
            _logger.LogInformation($"Media {media.MediaID} saved to database successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while saving media {media.MediaID} to database");
            // Don't throw - this is a background operation
        }
    }
}

