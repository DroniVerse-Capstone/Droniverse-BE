using Microsoft.AspNetCore.Http;

namespace Droniverse.Shared.DTOs.Response;

public class FileUploadDto
{
    public IFormFile File { get; set; } = null!;
}

