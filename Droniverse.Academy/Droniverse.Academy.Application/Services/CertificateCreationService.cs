using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class CertificateCreationService : ICertificateCreationService
{
    private readonly ICertificateService _certificateService;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly ICertificateImageService _certificateImageService;
    private readonly ICloudinaryService _cloudinaryService;

    public CertificateCreationService(
        ICertificateService certificateService,
        IdentityMicroserviceClient identityMicroserviceClient,
        ICertificateImageService certificateImageService,
        ICloudinaryService cloudinaryService)
    {
        _certificateService = certificateService;
        _identityMicroserviceClient = identityMicroserviceClient;
        _certificateImageService = certificateImageService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<CertificateResponseDTO> CreateCertificateWithGeneratedImageAsync(
        Guid courseId,
        Guid versionId,
        CreateCertificateRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var courseVersionTitleVN = await _certificateService.GetCourseVersionTitleVNAsync(courseId, versionId);

        var certificateTemplate = await _identityMicroserviceClient.GetCertificateTemplate();
        if (certificateTemplate == null || string.IsNullOrWhiteSpace(certificateTemplate.ImageUrl))
            throw new NotFoundException("Không tìm thấy mẫu chứng chỉ.");

        var generatedImage = await _certificateImageService.GenerateImageFromTemplateAsync(
            certificateTemplate.ImageUrl,
            courseVersionTitleVN,
            CertificateWriteFor.Course_Write,
            cancellationToken);

        var uploadedUrl = await _cloudinaryService.UploadImageAsync(
            generatedImage.Content,
            generatedImage.FileName,
            generatedImage.ContentType,
            "academy/certificates");

        return await _certificateService.CreateCertificateAsync(courseId, versionId, request, uploadedUrl);
    }
}
