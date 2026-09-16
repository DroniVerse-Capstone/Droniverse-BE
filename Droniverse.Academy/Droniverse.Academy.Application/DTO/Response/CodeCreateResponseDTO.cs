using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class CodeCreateResponseDTO
{
    public Guid CourseId { get; set; }
    public int TotalCreated { get; set; }
}
