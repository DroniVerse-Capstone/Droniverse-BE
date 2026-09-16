namespace Droniverse.Academy.Application.DTO.Response;

public sealed class GeneratedCertificateImageResponseDTO
{
    public required byte[] Content { get; init; }
    public required string FileName { get; init; }
    public string ContentType { get; init; } = "image/png";
}
