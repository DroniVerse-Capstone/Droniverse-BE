namespace Droniverse.Shared.DTOs;

/// <summary>
/// DTO rút gọn thông tin phiên bản khóa học, dùng cho cross-service communication.
/// </summary>
public class CourseVersionMiniResponseDTO
{
    public Guid CourseVersionID { get; set; }

    public Guid CourseID { get; set; }

    public string TitleVN { get; set; } = string.Empty;

    public string TitleEN { get; set; } = string.Empty;

    public int Version { get; set; }

    public string? ImageUrl { get; set; }
}
