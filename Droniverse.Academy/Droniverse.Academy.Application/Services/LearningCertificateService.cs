using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;

namespace Droniverse.Academy.Application.Services;

public sealed class LearningCertificateService
{
    private readonly IUnitOfWork _unitOfWork;

    public LearningCertificateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> TryIssueCertificateAsync(Enrollment enrollment, Guid userId, DateTime now)
    {
        if (enrollment.Status != EnrollStatus.COMPLETED)
            return false;

        return await IssueCertificateIfNeededAsync(enrollment.CourseVersionID, userId, now);
    }

    private async Task<bool> IssueCertificateIfNeededAsync(Guid courseVersionId, Guid userId, DateTime now)
    {
        var certificate = await _unitOfWork.Certificates.GetByConditionAsync(x => x.CourseVersionID == courseVersionId);
        if (certificate == null)
            return false;

        var existing = await _unitOfWork.UserCertificates.GetByConditionAsync(
            x => x.UserID == userId && x.CertificateID == certificate.CertificateID);

        if (existing != null)
            return false;

        var userCertificate = new UserCertificate
        {
            UserID = userId,
            CertificateID = certificate.CertificateID,
            CertificateUrl = certificate.ImageUrl,
            AchievedDate = now,
            Status = UserCertificateStatus.ACHIEVED
        };

        await _unitOfWork.UserCertificates.AddAsync(userCertificate);
        return true;
    }
}
