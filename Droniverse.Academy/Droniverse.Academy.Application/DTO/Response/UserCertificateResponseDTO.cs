using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public record UserCertificateResponseDTO(
    Guid CertificateID,
    Guid UserID,
    Guid SerialNumber,
    DateTime AchievedDate,
    UserCertificateStatus Status,
    CertificateResponseDTO? Certificate
    );
