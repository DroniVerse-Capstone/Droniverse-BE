using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Droniverse.Academy.Application.Services;

public sealed class LearningCertificateService  
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICertificateImageService _certificateImageService;
    private readonly IUserDisplayNameService _userDisplayNameService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IEmailService _emailService;
    private readonly ILogger<LearningCertificateService> _logger;

    public LearningCertificateService(
        IUnitOfWork unitOfWork,
        ICertificateImageService certificateImageService,
        IUserDisplayNameService userDisplayNameService,
        ICloudinaryService cloudinaryService,
        IEmailService emailService,
        ILogger<LearningCertificateService> logger)
    {
        _unitOfWork = unitOfWork;
        _certificateImageService = certificateImageService;
        _userDisplayNameService = userDisplayNameService;
        _cloudinaryService = cloudinaryService;
        _emailService = emailService;
        _logger = logger;
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

        var learner = await ResolveLearnerAsync(userId);
        var generatedImage = await _certificateImageService.GenerateImageFromTemplateAsync(
            certificate.ImageUrl,
            learner.FullName,
            CertificateWriteFor.User_Write);

        var uploadedUrl = await _cloudinaryService.UploadImageAsync(
            generatedImage.Content,
            generatedImage.FileName,
            generatedImage.ContentType,
            "academy/certificates");
        var userCertificate = new UserCertificate
        {
            UserID = userId,
            CertificateID = certificate.CertificateID,
            CertificateUrl = uploadedUrl,
            AchievedDate = now,
            Status = UserCertificateStatus.ACHIEVED
        };

        await _unitOfWork.UserCertificates.AddAsync(userCertificate);

        await SendCertificateIssuedEmailAsync(learner, uploadedUrl);
        return true;
    }

    private async Task<SimpleUserReponse> ResolveLearnerAsync(Guid userId)
    {
        var users = await _userDisplayNameService.GetListUserAsync([userId]);
        var learner = users.FirstOrDefault();

        if (learner == null)
            throw new NotFoundException("Không thể lấy thông tin học viên từ Identity service.");

        learner = learner with
        {
            FullName = learner.FullName?.Trim() ?? string.Empty,
            Email = learner.Email?.Trim() ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(learner.FullName))
            throw new NotFoundException("Không thể lấy thông tin họ tên học viên từ Identity service.");

        if (string.IsNullOrWhiteSpace(learner.Email))
            throw new NotFoundException("Không thể lấy thông tin email học viên từ Identity service.");

        return learner;
    }

    private async Task SendCertificateIssuedEmailAsync(SimpleUserReponse learner, string certificateImageUrl)
    {
        try
        {
            await SendCertificateIssuedEmailForTestAsync(learner, certificateImageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gửi email chúc mừng chứng chỉ thất bại cho user {UserId}", learner.UserId);
        }
    }

    public async Task SendCertificateIssuedEmailForTestAsync(SimpleUserReponse learner, string certificateImageUrl)
    {
        ArgumentNullException.ThrowIfNull(learner);

        if (string.IsNullOrWhiteSpace(learner.FullName))
            throw new ValidationException("Họ tên người nhận không hợp lệ.");

        if (string.IsNullOrWhiteSpace(learner.Email))
            throw new ValidationException("Email người nhận không hợp lệ.");

        if (string.IsNullOrWhiteSpace(certificateImageUrl))
            throw new ValidationException("Đường dẫn chứng chỉ không hợp lệ.");

        var subject = "Chúc mừng bạn đã hoàn thành khóa học!";
        var body = await BuildCertificateIssuedEmailBodyAsync(learner.FullName, certificateImageUrl);
        await _emailService.SendEmailAsync(learner.Email.Trim(), subject, body);
    }

    private async Task<string> BuildCertificateIssuedEmailBodyAsync(string fullName, string certificateImageUrl)
    {
        var htmlContent = await _emailService.LoadTemplateAsync("CertificateIssuedTemplate.html");
        var encodedFullName = WebUtility.HtmlEncode(fullName);
        var encodedCertificateImageUrl = WebUtility.HtmlEncode(certificateImageUrl);

        return htmlContent
            .Replace("{FullName}", encodedFullName)
            .Replace("{CertificateImageUrl}", encodedCertificateImageUrl);
    }
}
