using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Enums;

public class ClubCourse
{
    public Guid ClubID { get; private set; }
    public Guid CourseID { get; private set; }
    public int RemainingQuantity { get; private set; }
    public int TotalQuantity { get; private set; }
    public ClubCourseProfit ProfitType { get; private set; }

    public Club Club { get; private set; }

    private ClubCourse() { } // EF

    private ClubCourse(Guid clubId, Guid courseId, int totalQuantity, ClubCourseProfit profitType)
    {
        if (totalQuantity < 0)
            throw new ArgumentException("Total quantity must be >= 0");

        ClubID = clubId;
        CourseID = courseId;
        TotalQuantity = totalQuantity;
        RemainingQuantity = totalQuantity;
        ProfitType = profitType;
    }

    public static ClubCourse Create(Guid clubId, Guid courseId, int totalQuantity, ClubCourseProfit profitType)
    {
        return new ClubCourse(clubId, courseId, totalQuantity, profitType);
    }

    public void IncreaseCapacity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        TotalQuantity += quantity;
        RemainingQuantity += quantity;

        EnsureValidState();
    }

    public void Consume(int quantity = 1)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        if (RemainingQuantity < quantity)
            throw new InvalidOperationException("Not enough remaining quantity");

        RemainingQuantity -= quantity;
        EnsureValidState();
    }

    public void Restore(int quantity = 1)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        if (RemainingQuantity + quantity > TotalQuantity)
            throw new InvalidOperationException("Cannot exceed total quantity");

        RemainingQuantity += quantity;

        EnsureValidState();
    }

    public bool IsAvailable() => RemainingQuantity > 0;

    public void UpdateProfitType(ClubCourseProfit profitType)
    {
        ProfitType = profitType;
    }

    public void UpdateTotalQuantity(int totalQuantity)
    {
        if (totalQuantity < 0)
            throw new ArgumentException("Tổng số lượng phải lớn hơn hoặc bằng 0");

        if (totalQuantity < RemainingQuantity)
            throw new InvalidOperationException("Tổng số lượng không được nhỏ hơn số lượng còn lại");

        TotalQuantity = totalQuantity;
        EnsureValidState();
    }

    private void EnsureValidState()
    {
        if (RemainingQuantity < 0 || TotalQuantity < 0)
            throw new InvalidOperationException("Invalid quantity state");

        if (RemainingQuantity > TotalQuantity)
            throw new InvalidOperationException("Remaining cannot exceed total");
    }
}