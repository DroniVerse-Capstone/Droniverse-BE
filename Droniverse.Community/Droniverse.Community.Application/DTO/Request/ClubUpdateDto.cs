using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request;

/// <summary>
/// DTO cho việc cập nhật thông tin Club (không bao gồm Status)
/// </summary>
public record ClubUpdateDto
{
    [Required(ErrorMessage = "Tên tiếng Việt không được để trống")]
    [StringLength(200, ErrorMessage = "Tên tiếng Việt tối đa 200 ký tự")]
    public string NameVN { get; init; } = string.Empty;

    [Required(ErrorMessage = "Tên tiếng Anh không được để trống")]
    [StringLength(200, ErrorMessage = "Tên tiếng Anh tối đa 200 ký tự")]
    public string NameEN { get; init; } = string.Empty;

    public Guid? ImageMedia { get; init; }

    [Range(1, 10000, ErrorMessage = "Số lượng thành viên phải từ 1 đến 10000")]
    public int LimitParticipation { get; init; }

    [StringLength(5000, ErrorMessage = "Nội quy tiếng Việt tối đa 5000 ký tự")]
    public string? ClubPolicyVN { get; init; }

    [StringLength(5000, ErrorMessage = "Nội quy tiếng Anh tối đa 5000 ký tự")]
    public string? ClubPolicyEN { get; init; }

    [StringLength(2000, ErrorMessage = "Yêu cầu tham gia tối đa 2000 ký tự")]
    public string? ClubRequirement { get; init; }
}