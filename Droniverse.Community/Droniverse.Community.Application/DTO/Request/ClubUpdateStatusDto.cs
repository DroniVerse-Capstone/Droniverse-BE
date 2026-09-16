using Droniverse.Community.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request;

/// <summary>
/// DTO cho việc cập nhật trạng thái của Club
/// </summary>
public record ClubUpdateStatusDto
{
    /// <summary>
    /// Trạng thái mới của Club
    /// - INACTIVE (0): Tạm ngừng hoạt động
    /// - ACTIVE (1): Đang hoạt động
    /// - SUSPENDED (2): Bị đình chỉ
    /// - ARCHIVED (3): Đóng hẳn/Lưu trữ
    /// </summary>
    [Required(ErrorMessage = "Trạng thái không được để trống")]
    public ClubStatus Status { get; set; }

    /// <summary>
    /// Lý do thay đổi trạng thái 
    /// </summary>
    [StringLength(500, ErrorMessage = "Lý do tối đa 500 ký tự")]
    public string? Reason { get; set; }
}
