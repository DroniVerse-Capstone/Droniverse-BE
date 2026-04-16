using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository.SearchSpec;
using Droniverse.Shared.DTOs.Request;

namespace Droniverse.Academy.Domain.IRepository;
public interface ICodeRepository : IRepository<Code>
{
    Task<PaginationResult<IEnumerable<Code>>> GetAllCodesAsync(
        ICodeSearchSpec requestDTO, 
        int pageIndex, 
        int pageSize);

    Task<PaginationResult<IEnumerable<Code>>> GetCodesByClubAsync(
        Guid clubId,
        Guid courseId,
        GetAllCodesByClubSearchRequest request,
        int pageIndex,
        int pageSize);

    Task<PaginationResult<IEnumerable<Code>>> GetCodesByUserAsync(
        Guid userId,
        int pageIndex,
        int pageSize,
        bool? isUsed = null);

    Task<IEnumerable<Code>> GetByCodeIdsAsync(IEnumerable<string> codeIds);

    Task<List<Guid>> GetOwnedUserIdsByClubAndCourseAsync(
        Guid clubId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveUnusedOwnedCodeAsync(
        Guid clubId,
        Guid courseId,
        Guid userId,
        DateTime now,
        CancellationToken cancellationToken = default);
}

