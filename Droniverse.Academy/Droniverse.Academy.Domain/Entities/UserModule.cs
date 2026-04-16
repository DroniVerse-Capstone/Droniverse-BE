namespace Droniverse.Academy.Domain.Entities;
public class UserModule
{
    public Guid UserModuleID { get; set; } //char(36)
    public Guid UserID { get; set; } //char(36) // reference to User
    public Module Module { get; set; }
    public Guid ModuleID { get; set; } //char(36) // reference to Module
    public DateTime EnrollDate { get; set; } //datetime
    public DateTime? CompleteDate { get; set; } //datetime
    public float Progress { get; set; } //float
    public bool IsCompleted { get; set; } //boolean
}
