namespace Droniverse.Community.Application.DTO.Request;

public class MoveMediaFolderRequest
{
    /// <summary>
    /// ID của tệp media cần di chuyển
    /// </summary>
    public Guid MediaId { get; set; }

    /// <summary>
    /// Thư mục đích trên Cloudinary (vd: droniverse/Club/Club123)
    /// </summary>
    public string Folder { get; set; } = null!;
}
