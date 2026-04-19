using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Services.Duplication.Models;

public class CourseVersionDuplicationContext
{
    // Version nguồn 
    public required CourseVersion SourceVersion { get; init; }
    // Version đích 
    public required CourseVersion DuplicatedVersion { get; init; }
    public required Guid CurrentUserId { get; init; }
    public required DateTime Now { get; init; }

    // Mapping từ ID cũ sang ID mới cho các bảng liên quan, để sử dụng khi sao chép các bảng có quan hệ khóa ngoại.
    // Mới khởi tạo, các Dictionary chứa Id của Version nguồn, sau đó được điền dần khi sao chép từng bảng (Module, Theory, Quiz, Lab), để đảm bảo khi sao chép các bảng có quan hệ khóa ngoại (ví dụ: Lesson có khóa ngoại đến Module) thì có thể lấy được ID mới của Module đã được sao chép.
    public Dictionary<Guid, Guid> ModuleIdMap { get; } = new();
    public Dictionary<Guid, Guid> TheoryIdMap { get; } = new();
    public Dictionary<Guid, Guid> QuizIdMap { get; } = new();
    public Dictionary<Guid, Guid> LabIdMap { get; } = new();
    public Dictionary<Guid, Guid> StructureSimulatorIdMap { get; } = new();
    public Dictionary<Guid, Guid> FlightSimulatorIdMap { get; } = new();
    public List<(Guid SourceLabId, Guid NewLabId)> LabContentSyncQueue { get; } = new();
}
