using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

namespace Droniverse.Academy.Application.Services;

public class UserCertificateService : IUserCertificateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IClock _clock;

    public UserCertificateService(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _clock = clock;
    }

    public async Task GrantCertificateToUserAsync(Guid certificateId, Guid userId)
    {
        var cert = await _unitOfWork.Certificates.GetByIdAsync(certificateId);
        if (cert == null)
            throw new BaseException("Không tìm thấy chứng chỉ.", "NOT_FOUND");

        // prevent duplicate
        var existing = await _unitOfWork.UserCertificates.GetByConditionAsync(uc => uc.CertificateID == certificateId && uc.UserID == userId);
        if (existing != null)
            throw new ValidationException("Người dùng đã được cấp chứng chỉ này.");

        var uc = new UserCertificate
        {
            CertificateID = certificateId,
            UserID = userId,
            SerialNumber = Guid.NewGuid(),
            AchievedDate = _clock.Now,
            Status = UserCertificateStatus.ACHIEVED
        };

        await _unitOfWork.UserCertificates.AddAsync(uc);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<UserCertificateResponseDTO> GetUserCertificateAsync(Guid userId, Guid certificateId)
    {
        var uc = await _unitOfWork.UserCertificates.GetByConditionAsync(x => x.UserID == userId && x.CertificateID == certificateId, includeProperties: "Certificate");
        if (uc == null)
            throw new BaseException("Không tìm thấy chứng chỉ của người dùng.", "NOT_FOUND");

        return _mapper.Map<UserCertificateResponseDTO>(uc);
    }

    public async Task<PaginationResult<IEnumerable<UserCertificateResponseDTO>>> GetUserCertificatesAsync(Guid userId, int pageIndex = 1, int pageSize = 50)
    {
        var result = await _unitOfWork.UserCertificates.GetAllAsync(x => x.UserID == userId, null, pageIndex, pageSize, includeProperties: "Certificate");
        var mapped = result.Data.Select(x => _mapper.Map<UserCertificateResponseDTO>(x)).ToList();
        return new PaginationResult<IEnumerable<UserCertificateResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<PaginationResult<IEnumerable<UserCertificateResponseDTO>>> GetUsersByCertificateAsync(Guid certificateId, int pageIndex = 1, int pageSize = 50)
    {
        var result = await _unitOfWork.UserCertificates.GetAllAsync(x => x.CertificateID == certificateId, null, pageIndex, pageSize, includeProperties: "Certificate");
        var mapped = result.Data.Select(x => _mapper.Map<UserCertificateResponseDTO>(x)).ToList();
        return new PaginationResult<IEnumerable<UserCertificateResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task RevokeUserCertificateAsync(Guid userId, Guid certificateId)
    {
        var uc = await _unitOfWork.UserCertificates.GetByConditionAsync(x => x.UserID == userId && x.CertificateID == certificateId);
        if (uc == null)
            throw new BaseException("Không tìm thấy chứng chỉ của người dùng.", "NOT_FOUND");

        uc.Status = UserCertificateStatus.REVOKED;
        await _unitOfWork.UserCertificates.UpdateAsync(uc);
        await _unitOfWork.SaveChangesAsync();
    }
}
