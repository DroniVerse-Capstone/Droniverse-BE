using Droniverse.Academy.Domain.Enums;
namespace Droniverse.Academy.Domain.IRepository.SearchSpec;

public interface ICodeSearchSpec
{
    CodeStatus? Status { get; }
    CodeUsageStatus? CodeUsageStatus { get; }
}

