using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

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

    public async Task<CertificateResponseDTO> CreateCertificateAsync(Guid courseId, Guid versionId, CreateCertificateRequestDTO request, string imageUrl)
    {
        await GetValidatedCourseVersionAsync(courseId, versionId);

        var cert = _mapper.Map<Certificate>(request);
        cert.CertificateID = Guid.NewGuid();
        cert.CourseVersionID = versionId;
        cert.ImageUrl = imageUrl;
        cert.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Certificates.AddAsync(cert);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CertificateResponseDTO>(cert);
        await PopulateUsersAsync(response, cert.CreateBy, cert.UpdateBy);

        return response;
    }

    public async Task<string> GetCourseVersionTitleVNAsync(Guid courseId, Guid versionId)
    {
        var cv = await GetValidatedCourseVersionAsync(courseId, versionId);
        return cv.TitleVN;
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
        await PopulateUsersAsync(response, cert.CreateBy, cert.UpdateBy);

        return response;
    }

    public async Task<CertificateResponseDTO> GetCertificateByIdAsync(Guid certificateId)
    {
        var cert = await _unitOfWork.Certificates.GetByIdAsync(certificateId);
        if (cert == null)
            throw new BaseException("Không tìm thấy chứng chỉ.", "NOT_FOUND");

        var response = _mapper.Map<CertificateResponseDTO>(cert);
        await PopulateUsersAsync(response, cert.CreateBy, cert.UpdateBy);

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
        await PopulateUsersAsync(response, cert.CreateBy, cert.UpdateBy);

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

        var entities = result.Data.ToList();
        var data = entities
            .Select(c => _mapper.Map<CertificateResponseDTO>(c))
            .ToList();

        var userLookup = await BuildUserLookupAsync(entities);
        PopulateMappedCertificatesUsers(entities, data, userLookup);

        return data
            .OrderBy(c => ids.IndexOf(c.CertificateID))
            .ToList();
    }

    public async Task<IEnumerable<SimpleCertificateResponse>> GetCertificatesBulkAsync(IEnumerable<Guid> certificateIds, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Certificates.GetSimpleCertificatesByIdsAsync(certificateIds, cancellationToken);
    }

    private async Task PopulateUsersAsync(CertificateResponseDTO certificate, Guid createBy, Guid updateBy)
    {
        var (creator, updater) = await _userDisplayNameService.ResolveCreatorUpdaterAsync(createBy, updateBy);
        certificate.Creator = creator;
        certificate.Updater = updater;
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<Certificate> certificates)
    {
        var userIds = certificates
            .SelectMany(c => new[] { c.CreateBy, c.UpdateBy })
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var lookup = await _userDisplayNameService.ResolveUsersDisplayNameAsync(userIds);
        return lookup.ToDictionary(x => x.Key, x => x.Value);
    }

    private static void PopulateMappedCertificatesUsers(
        IEnumerable<Certificate> entities,
        IEnumerable<CertificateResponseDTO> dtos,
        IReadOnlyDictionary<Guid, SimpleUserReponse?> userLookup)
    {
        foreach (var (entity, dto) in entities.Zip(dtos))
        {
            if (userLookup.TryGetValue(entity.CreateBy, out var creator))
            {
                dto.Creator = creator;
            }

            if (userLookup.TryGetValue(entity.UpdateBy, out var updater))
            {
                dto.Updater = updater;
            }
        }
    }

    private async Task<CourseVersion> GetValidatedCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(
            v => v.CourseVersionID == versionId && v.CourseID == courseId,
            includeProperties: "Certificate");

        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (cv.Certificate != null)
            throw new ValidationException("Phiên bản khóa học đã có chứng chỉ.");

        return cv;
    }
}
