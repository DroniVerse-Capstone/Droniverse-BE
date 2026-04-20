using Droniverse.Identity.Domain.Enums;
namespace Droniverse.Identity.Domain.Entities;
public class Account
{
    public Guid UserID { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? RefreshToken{ get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Role Role { get; set; }
    public Guid RoleID { get; set; }
    public virtual UserInfo UserInfo { get; set; }
    

}

