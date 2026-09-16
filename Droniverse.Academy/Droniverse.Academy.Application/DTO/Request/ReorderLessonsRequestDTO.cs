namespace Droniverse.Academy.Application.DTO.Request;

public class ReorderLessonsRequestDTO
{
    public List<ReorderLessonItemDTO> Lessons { get; set; } = [];
}

public class ReorderLessonItemDTO
{
    public Guid LessonID { get; set; }
    public int OrderIndex { get; set; }
}
