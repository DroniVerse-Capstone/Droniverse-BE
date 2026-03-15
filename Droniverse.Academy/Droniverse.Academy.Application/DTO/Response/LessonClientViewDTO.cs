using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class LessonClientViewDTO
{
    public LessonType Type { get; set; }
    public Guid ReferenceID { get; set; }
}
