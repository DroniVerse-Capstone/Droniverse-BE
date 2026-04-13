using System.ComponentModel.DataAnnotations;

namespace Droniverse.Shared.DTOs.Request;

public class AssignCodeRequest
{
    [Required(ErrorMessage = "Cần có mã code của khóa học")]
    public required string CodeId { get; set; }
    [Required(ErrorMessage = "Cần có mã người dùng")]
    public Guid UserId { get; set; }
    [Required(ErrorMessage = "Cần biết là có nên gửi mail hay không")]
    public bool SendEmail { get; set; }
}
