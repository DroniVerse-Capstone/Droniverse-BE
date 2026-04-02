using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class CreateUserLessonRequestDTO
{
    public Guid LessonID { get; set; }
    public UserLessonStatus Status { get; set; } = UserLessonStatus.INCOMPLETED;
    public float Progress { get; set; }
}
