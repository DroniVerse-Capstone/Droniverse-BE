using Microsoft.AspNetCore.Http;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Request;

public class FileUploadMediaDto
{
    /// <summary>
    /// File upload (image or video)
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Media type: IMAGE, VIDEO, GIF, AUDIO
    /// </summary>
    public MediaTypeEnum MediaType { get; set; }
}
