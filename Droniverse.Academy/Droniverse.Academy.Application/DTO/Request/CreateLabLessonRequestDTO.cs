namespace Droniverse.Academy.Application.DTO.Request;

public class CreateLabLessonRequestDTO
{
    public Guid ModuleID { get; set; }
    public int? OrderIndex { get; set; }
}
