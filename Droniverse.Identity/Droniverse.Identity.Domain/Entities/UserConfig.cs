using Droniverse.Identity.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Droniverse.Identity.Domain.Entities;
public class UserConfig
{
    [Key]
    [ForeignKey("UserInfo")]
    public Guid UserID { get; set; }
    public LanguageOptions Language { get; set; }
    public UserInfo UserInfo { get; set; }
}

