using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class CreateLessonRequestDTO
{
    public LessonType Type { get; set; }
    public Guid ReferenceID { get; set; }
}
