namespace Droniverse.Shared.DTOs.Request;

public class AssignCodeRequest
{
    public Guid UserId { get; set; }
    public bool SendEmail { get; set; }
}
