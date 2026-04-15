using Droniverse.Academy.Domain.Enums;

using System.Diagnostics.CodeAnalysis;

namespace Droniverse.Academy.Domain.Entities;

public class Code
{
    public required string CodeID { get; set; }
    public required Guid ClubID { get; set; }
    public Guid CourseID { get; set; }
    public Course Course { get; set; }

    public DateTime ExpireDate { get; set; }
    public CodeStatus Status { get; set; } = CodeStatus.Active;

    // Ownership
    public Guid? OwnedUserID { get; set; }

    // Usage
    public Guid? UsedByUserID { get; set; }
    public DateTime? UsedDate { get; set; }
    public ICollection<CodeUsage> CodeUsages { get; set; } = [];

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    [SetsRequiredMembers]
    public Code(
    string codeId,
    Guid clubId,
    Guid courseId,
    DateTime expireDate,
    Guid createdBy,
    DateTime now)
    {
        if (expireDate <= now)
            throw new InvalidOperationException("Ngày hết hạn phải lớn hơn thời gian hiện tại.");

        CodeID = codeId;
        ClubID = clubId;
        CourseID = courseId;
        ExpireDate = expireDate;

        CreatedBy = createdBy;
        CreatedAt = now;

        Status = CodeStatus.Active;
    }

    // EF Core cần constructor rỗng
    [SetsRequiredMembers]
    private Code() { }

    // =========================
    // DDD BEHAVIOR METHODS
    // =========================

    public void AssignToUser(Guid userId, DateTime now)
    {
        if (Status != CodeStatus.Active)
            throw new InvalidOperationException("Code không thể sử dụng.");

        if (IsExpired(now))
            throw new InvalidOperationException("Code đã quá hạn sử dụng.");

        if (OwnedUserID != null)
            throw new InvalidOperationException("Code đã được gán cho người dùng rồi.");

        OwnedUserID = userId;
        UpdatedAt = now;
    }

    public void Redeem(Guid userId, DateTime now)
    {
        if (Status != CodeStatus.Active)
            throw new InvalidOperationException("Mã code không ở trạng thái hợp lệ để sử dụng.");

        if (IsExpired(now))
            throw new InvalidOperationException("Mã code đã hết hạn.");

        if (UsedByUserID != null)
            throw new InvalidOperationException("Mã code đã được sử dụng.");

        UsedByUserID = userId;
        UsedDate = now;
        Status = CodeStatus.Used;

        UpdatedAt = now;
    }

    public void Expire(DateTime now)
    {
        Status = CodeStatus.Expired;
        UpdatedAt = now;
    }

    public void Disable(Guid adminId, DateTime now)
    {
        Status = CodeStatus.Disabled;
        UpdatedBy = adminId;
        UpdatedAt = now;
    }

    public bool IsExpired(DateTime now)
    {
        return now > ExpireDate;
    }

    public bool IsAvailable(DateTime now)
    {
        return Status == CodeStatus.Active && !IsExpired(now);
    }

    public bool IsUsed()
    {
        return UsedByUserID.HasValue && UsedByUserID != Guid.Empty;
    }
}