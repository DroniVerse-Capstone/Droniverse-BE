using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Abstractions;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class CertificateService : ICertificateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public CertificateService(IUnitOfWork unitOfWork, ICurrentUser current, IClock clock, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = current;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task<CertificateResponseDTO> CreateCertificateAsync(Guid courseId, Guid versionId, CreateCertificateRequestDTO request)
    {
        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId, includeProperties: "Certificate");
        if (cv == null)
            throw new BaseException("Course version not found.", "NOT_FOUND");

        if (cv.Certificate != null)
            throw new ValidationException("Course version already has a certificate.");

        var cert = new Certificate
        {
            CertificateID = Guid.NewGuid(),
            CourseVersionID = versionId,
            CertificateName = request.CertificateName,
            ImageUrl = request.ImageUrl,
            LogoCertificate = request.LogoCertificate,
            Description = request.Description,
            Signature = request.Signature,
            AuthorName = request.AuthorName,
            CreateAt = _clock.Now,
            CreateBy = _currentUser.UserId,
            UpdateAt = _clock.Now,
            UpdateBy = _currentUser.UserId
        };

        await _unitOfWork.Certificates.AddAsync(cert);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CertificateResponseDTO>(cert);
    }

    public async Task DeleteCertificateAsync(Guid courseId, Guid versionId, Guid certificateId)
    {
        var cert = await _unitOfWork.Certificates.GetByIdAsync(certificateId);
        if (cert == null)
            throw new BaseException("Certificate not found.", "NOT_FOUND");

        if (cert.CourseVersionID != versionId)
            throw new ValidationException("Certificate does not belong to the given course version.");

        await _unitOfWork.Certificates.DeleteAsync(cert);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CertificateResponseDTO> GetCertificateAsync(Guid courseId, Guid versionId)
    {
        var cert = await _unitOfWork.Certificates.GetByConditionAsync(c => c.CourseVersionID == versionId);
        if (cert == null)
            throw new BaseException("Certificate not found.", "NOT_FOUND");

        return _mapper.Map<CertificateResponseDTO>(cert);
    }

    public async Task<CertificateResponseDTO> GetCertificateByIdAsync(Guid certificateId)
    {
        var cert = await _unitOfWork.Certificates.GetByIdAsync(certificateId);
        if (cert == null)
            throw new BaseException("Certificate not found.", "NOT_FOUND");

        return _mapper.Map<CertificateResponseDTO>(cert);
    }

    public async Task<CertificateResponseDTO> UpdateCertificateAsync(Guid courseId, Guid versionId, Guid certificateId, UpdateCertificateRequestDTO request)
    {
        var cert = await _unitOfWork.Certificates.GetByIdAsync(certificateId);
        if (cert == null)
            throw new BaseException("Certificate not found.", "NOT_FOUND");

        if (cert.CourseVersionID != versionId)
            throw new ValidationException("Certificate does not belong to the given course version.");

        cert.CertificateName = request.CertificateName;
        cert.ImageUrl = request.ImageUrl;
        cert.LogoCertificate = request.LogoCertificate;
        cert.Description = request.Description;
        cert.Signature = request.Signature;
        cert.AuthorName = request.AuthorName;
        cert.UpdateAt = _clock.Now;
        cert.UpdateBy = _currentUser.UserId;

        await _unitOfWork.Certificates.UpdateAsync(cert);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CertificateResponseDTO>(cert);
    }

    public async Task<IEnumerable<CertificateResponseDTO>> GetCertificatesByIdsAsync(IEnumerable<Guid> certificateIds)
    {
        var ids = certificateIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return [];

        var result = await _unitOfWork.Certificates.GetAllAsync(
            filter: c => ids.Contains(c.CertificateID),
            pageIndex: 1,
            pageSize: ids.Count);

        var data = result.Data
            .Select(c => _mapper.Map<CertificateResponseDTO>(c))
            .ToList();

        return data
            .OrderBy(c => ids.IndexOf(c.CertificateID))
            .ToList();
    }
}
