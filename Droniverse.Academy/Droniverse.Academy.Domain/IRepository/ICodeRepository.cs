using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository.SearchSpec;

namespace Droniverse.Academy.Domain.IRepository;
public interface ICodeRepository : IRepository<Code>
{
    Task<PaginationResult<IEnumerable<Code>>> GetAllCodesAsync(
        ICodeSearchSpec requestDTO, 
        int pageIndex, 
        int pageSize);
}

