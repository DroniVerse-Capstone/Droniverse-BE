namespace Droniverse.Academy.Application.DTO.Response;

public class UserModuleResponseDTO
{
    public Guid UserID { get; set; }
    public Guid ModuleID { get; set; }
    public DateTime EnrollDate { get; set; }
    public DateTime? CompleteDate { get; set; }
    public float Progress { get; set; }
    public bool IsCompleted { get; set; }
}
