using System.ComponentModel.DataAnnotations;

namespace Droniverse.Shared.DTOs.Request;

public class GenerateWithAssignCodeRequestDTO
{
    [Required(ErrorMessage = "Không thể thiếu mã câu lạc bộ")]
    public required Guid ClubId { get; set; }

    [Required(ErrorMessage = "Không thể thiếu mã khóa học")]
    public required Guid CourseId { get; set; }

    [Range(1, 50, ErrorMessage = "Tạo từ 1 tới 50 mã")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Không thể thiếu email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Không thể thiếu tên người dùng")]
    public required string FullName { get; set; }
}
