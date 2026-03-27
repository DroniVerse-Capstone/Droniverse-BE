using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

namespace Droniverse.Academy.Application.Services;

public class CertificateService : ICertificateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;
    private readonly IUserDisplayNameService _userDisplayNameService;

    public CertificateService(IUnitOfWork unitOfWork, ICurrentUserService current, IClock clock, IMapper mapper, IUserDisplayNameService userDisplayNameService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = current;
        _clock = clock;
        _mapper = mapper;
        _userDisplayNameService = userDisplayNameService;
    }

    public async Task<CertificateResponseDTO> CreateCertificateAsync(Guid courseId, Guid versionId, CreateCertificateRequestDTO request)
    {
        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId, includeProperties: "Certificate");
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (cv.Certificate != null)
            throw new ValidationException("Phiên bản khóa học đã có chứng chỉ.");

        var cert = _mapper.Map<Certificate>(request);
        cert.CertificateID = Guid.NewGuid();
        cert.CourseVersionID = versionId;
        cert.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Certificates.AddAsync(cert);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CertificateResponseDTO>(cert);
        await PopulateUsersAsync(response);

        return response;
    }

    public async Task DeleteCertificateAsync(Guid courseId, Guid versionId, Guid certificateId)
    {
        var cert = await _unitOfWork.Certificates.GetByIdAsync(certificateId);
        if (cert == null)
            throw new BaseException("Không tìm thấy chứng chỉ.", "NOT_FOUND");

        if (cert.CourseVersionID != versionId)
            throw new ValidationException("Chứng chỉ không thuộc phiên bản khóa học đã cung cấp.");

        await _unitOfWork.Certificates.DeleteAsync(cert);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CertificateResponseDTO> GetCertificateAsync(Guid courseId, Guid versionId)
    {
        var cert = await _unitOfWork.Certificates.GetByConditionAsync(c => c.CourseVersionID == versionId);
        if (cert == null)
            throw new BaseException("Không tìm thấy chứng chỉ.", "NOT_FOUND");

        var response = _mapper.Map<CertificateResponseDTO>(cert);
        await PopulateUsersAsync(response);

        return response;
    }

    public async Task<CertificateResponseDTO> GetCertificateByIdAsync(Guid certificateId)
    {
        var cert = await _unitOfWork.Certificates.GetByIdAsync(certificateId);
        if (cert == null)
            throw new BaseException("Không tìm thấy chứng chỉ.", "NOT_FOUND");

        var response = _mapper.Map<CertificateResponseDTO>(cert);
        await PopulateUsersAsync(response);

        return response;
    }

    public async Task<CertificateResponseDTO> UpdateCertificateAsync(Guid courseId, Guid versionId, Guid certificateId, UpdateCertificateRequestDTO request)
    {
        var cert = await _unitOfWork.Certificates.GetByIdAsync(certificateId);
        if (cert == null)
            throw new BaseException("Không tìm thấy chứng chỉ.", "NOT_FOUND");

        if (cert.CourseVersionID != versionId)
            throw new ValidationException("Chứng chỉ không thuộc phiên bản khóa học đã cung cấp.");

        _mapper.Map(request, cert);
        cert.SetAuditOnUpdate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Certificates.UpdateAsync(cert);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CertificateResponseDTO>(cert);
        await PopulateUsersAsync(response);

        return response;
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

        await Task.WhenAll(data.Select(PopulateUsersAsync));

        return data
            .OrderBy(c => ids.IndexOf(c.CertificateID))
            .ToList();
    }

    private async Task PopulateUsersAsync(CertificateResponseDTO certificate)
    {
        var (creator, updater) = await _userDisplayNameService.ResolveCreatorUpdaterAsync(certificate.CreateBy, certificate.UpdateBy);
        certificate.Creator = creator;
        certificate.Updater = updater;
    }
}
