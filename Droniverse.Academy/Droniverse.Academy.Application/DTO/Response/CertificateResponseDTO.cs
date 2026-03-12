namespace Droniverse.Academy.Application.DTO.Response;

public record CertificateResponseDTO(
    Guid CertificateID,
    Guid CourseVersionID,
    string CertificateName,
    string ImageUrl,
    string LogoCertificate,
    string Description,
    string Signature,
    string AuthorName,
    DateTime CreateAt,
    Guid CreateBy,
    DateTime UpdateAt,
    Guid UpdateBy
    );
