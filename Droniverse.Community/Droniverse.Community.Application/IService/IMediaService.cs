using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Community.Application.IService;

public interface IMediaService
{
    Task<IEnumerable<MediaResponseDto>> GetAllMedia();
    Task<MediaResponseDto?> GetMediaById(Guid id);
    Task<MediaResponseDto> UploadTempMedia(FileUploadMediaDto dto, ICloudinaryService cloudinaryService);
}

