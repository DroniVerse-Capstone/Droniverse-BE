namespace Droniverse.Identity.Domain.Entities;
public class Permission
{
    public Guid PermissionID { get; set; }
    public string PermissionName { get; set; }
    public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}
