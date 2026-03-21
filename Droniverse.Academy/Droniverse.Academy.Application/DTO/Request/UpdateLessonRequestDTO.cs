using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class UpdateLessonRequestDTO
{
    public int? OrderIndex { get; set; }
    public LessonType Type { get; set; }
    public Guid ReferenceID { get; set; }
}
