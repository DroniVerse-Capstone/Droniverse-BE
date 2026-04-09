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

    //Tạo ClubCourse mới với số lượng ban đầu và loại lợi nhuận (Khi ClubManager mua code)
    public static ClubCourse Create(Guid clubId, Guid courseId, int totalQuantity, ClubCourseProfit profitType)
    {
        return new ClubCourse(clubId, courseId, totalQuantity, profitType);
    }

    //Khi ClubManager mua thêm code của 1 khóa học mà đã có ClubCourse, chỉ cần tăng số lượng mà không cần tạo mới
    public void IncreaseCapacity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        TotalQuantity += quantity;
        RemainingQuantity += quantity;

        EnsureValidState();
    }

    //Khi clubmember nhập mã code để tham gia khóa học, giảm số lượng còn lại đi 1 đơn vị (hoặc nhiều hơn nếu cho phép nhập nhiều code cùng lúc)
    public void Consume(int quantity = 1)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        if (RemainingQuantity < quantity)
            throw new InvalidOperationException("Not enough remaining quantity");

        RemainingQuantity -= quantity;
        EnsureValidState();
    }

    //Khi 1 học viên hủy khóa học hoặc code bị lỗi
    public void Restore(int quantity = 1)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        if (RemainingQuantity + quantity > TotalQuantity)
            throw new InvalidOperationException("Cannot exceed total quantity");

        RemainingQuantity += quantity;

        EnsureValidState();
    }

    //kiểm tra xem còn code để cấp không
    public bool IsAvailable() => RemainingQuantity > 0;

    //thay đổi loại lợi nhuận cho clb
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

    //Kiểm tra lại nội bộ mỗi khi thay đổi số lượng để đảm bảo không rơi vào trạng thái không hợp lệ
    private void EnsureValidState()
    {
        if (RemainingQuantity < 0 || TotalQuantity < 0)
            throw new InvalidOperationException("Invalid quantity state");

        if (RemainingQuantity > TotalQuantity)
            throw new InvalidOperationException("Remaining cannot exceed total");
    }
}