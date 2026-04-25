namespace Droniverse.Academy.Application.DTO.Request;

public class SubmitUserAssignmentRequestDTO
{
    public Guid MediaID { get; set; }
    public string Description { get; set; } = string.Empty;
}
