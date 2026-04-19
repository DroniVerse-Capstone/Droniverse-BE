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

    // Usage
    public Guid? UsedByUserID { get; set; }
    public DateTime? UsedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

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

    }

    public void Expire(DateTime now)
    {
        Status = CodeStatus.Expired;
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