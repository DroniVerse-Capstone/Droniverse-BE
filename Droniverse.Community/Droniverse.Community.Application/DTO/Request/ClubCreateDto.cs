using System.ComponentModel.DataAnnotations;
using Droniverse.Community.Domain.Enums;

public record ClubCreateDto
{
    [Required(ErrorMessage = "Tên tiếng Việt không được để trống")]
    [StringLength(200, ErrorMessage = "Tên tiếng Việt tối đa 200 ký tự")]
    public string NameVN { get; init; } = string.Empty;

    [Required(ErrorMessage = "Tên tiếng Anh không được để trống")]
    [StringLength(200, ErrorMessage = "Tên tiếng Anh tối đa 200 ký tự")]
    public string NameEN { get; init; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Mô tả tiếng Việt tối đa 1000 ký tự")]
    public string? DescriptionVN { get; init; }

    [StringLength(1000, ErrorMessage = "Mô tả tiếng Anh tối đa 1000 ký tự")]
    public string? DescriptionEN { get; init; }

    [Required(ErrorMessage = "Trạng thái không được để trống")]
    public ClubStatus Status { get; init; }
    public bool IsPublic { get; init; }

    [Range(1, 10000, ErrorMessage = "Số lượng thành viên phải từ 1 đến 10000")]
    public int LimitParticipation { get; init; }

    [Range(1, 100, ErrorMessage = "Số lượng quản lý phải từ 1 đến 100")]
    public int LimitClubManagers { get; init; }

    [Required(ErrorMessage = "CreatedBy không được để trống")]
    public Guid CreatedBy { get; init; }

    [MinLength(1, ErrorMessage = "Phải có ít nhất một danh mục")]
    public List<Guid> CategoryIDs { get; init; } = new();
}