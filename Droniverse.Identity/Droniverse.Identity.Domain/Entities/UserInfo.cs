using Droniverse.Identity.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Droniverse.Identity.Domain.Entities;
public class UserInfo
{
    [Key]
    [ForeignKey("Account")]
    public Guid UserID { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Phone { get; set; }
    public GenderOptions Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public virtual Account Account { get; set; }
    public virtual UserConfig UserConfig { get; set; }
}
