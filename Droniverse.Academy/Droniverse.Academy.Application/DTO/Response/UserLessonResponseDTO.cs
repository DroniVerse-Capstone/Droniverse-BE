using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class UserLessonResponseDTO
{
    public Guid UserLessonID { get; set; }
    public Guid LessonID { get; set; }
    public Guid UserID { get; set; }
    public UserLessonStatus Status { get; set; }
    public float Progress { get; set; }
    public DateTime? LastAccessDate { get; set; }
}
