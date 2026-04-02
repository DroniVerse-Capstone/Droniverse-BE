namespace Droniverse.Academy.Application.DTO.Request;

public class CreateUserModuleRequestDTO
{
    public Guid ModuleID { get; set; }
    public float Progress { get; set; }
    public bool IsCompleted { get; set; }
}
