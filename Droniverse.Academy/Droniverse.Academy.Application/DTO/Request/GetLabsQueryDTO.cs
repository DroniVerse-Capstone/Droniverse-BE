using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class GetLabsQueryDTO
{
    public LabType? Type { get; set; }
    public LabStatus? Status { get; set; }
    public string? SearchTerm { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
