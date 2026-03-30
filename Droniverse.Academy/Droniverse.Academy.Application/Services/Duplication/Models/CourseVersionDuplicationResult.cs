using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Services.Duplication.Models;

public class CourseVersionDuplicationResult
{
    //Đây là kết quả sau khi đã tạo xong bản sao 
    public required CourseVersion DuplicatedVersion { get; init; }
    //Đây là danh sách các Lab cần đồng bộ nội dung sau khi đã sao chép xong, bao gồm ID của Lab nguồn và ID của Lab mới được tạo ra. Danh sách này được điền trong quá trình sao chép Module, Theory, Quiz, Lab, khi gặp bảng Lab thì sẽ lưu vào danh sách này để sau khi hoàn thành việc sao chép tất cả các bảng thì sẽ tiến hành đồng bộ nội dung cho các Lab này.
    public required IReadOnlyCollection<(Guid SourceLabId, Guid NewLabId)> LabContentSyncQueue { get; init; }
}
