using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Response.Mongo;

/// <summary>
/// DTO chi tiết đơn hàng của một user, dùng cho dashboard admin.
/// </summary>
public class UserOrderDetailResponseDto
{
    /// <summary>Thông tin rút gọn phiên bản khóa học được mua.</summary>
    public CourseVersionMiniResponseDTO? CourseVersion { get; set; }

    /// <summary>Câu lạc bộ thực hiện đơn hàng.</summary>
    public ClubMiniResponse? Club { get; set; }

    /// <summary>Thời điểm tạo đơn hàng.</summary>
    public DateTime Time { get; set; }

    /// <summary>Tổng số tiền của đơn hàng.</summary>
    public decimal Amount { get; set; }

    /// <summary>Trạng thái đơn hàng.</summary>
    public OrderStatus Status { get; set; }
}
