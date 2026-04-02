using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Request;

public class UpdateUserLessonRequestDTO
{
    public UserLessonStatus? Status { get; set; }
    public float? Progress { get; set; }
    public DateTime? LastAccessDate { get; set; }
}
