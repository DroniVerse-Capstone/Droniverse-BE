using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public enum CertificateWriteFor
{
    Course_Write = 0,
    User_Write = 1
}

public interface ICertificateImageService
{
    Task<GeneratedCertificateImageResponseDTO> GenerateImageFromTemplateAsync(
        string templateImageUrl,
        string content,
        CertificateWriteFor certificateWriteFor,
        CancellationToken cancellationToken = default);
}
