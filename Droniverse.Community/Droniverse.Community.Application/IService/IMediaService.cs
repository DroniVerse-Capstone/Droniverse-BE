using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Community.Application.IService;

public interface IMediaService
{
    Task<IEnumerable<MediaResponseDto>> GetAllMedia();
    Task<MediaResponseDto?> GetMediaById(Guid id);
    Task<Media?> GetFullMedia(Guid id);
    Task<MediaResponseDto> GetMediaByUrl(string imageUrl);
    Task<MediaResponseDto> UploadTempMedia(FileUploadMediaDto dto);
    Task UploadMedia(Media media, string folder);
    Task<IEnumerable<MediaMiniResponse>> GetMiniResponse(IEnumerable<Guid>? mediaIds);
}

