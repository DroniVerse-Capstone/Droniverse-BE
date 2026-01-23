namespace Droniverse.Identity.Domain.Entities;
public class Role
{
    public Guid RoleID { get; set; }
    public string RoleName { get; set; }
    public string? Description { get; set; }

    public ICollection<Account> Accounts { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}
