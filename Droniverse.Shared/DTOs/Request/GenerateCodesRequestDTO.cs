using System.ComponentModel.DataAnnotations;

namespace Droniverse.Shared.DTOs.Request
{
    public record GenerateCodesRequestDTO
    {
        [Required(ErrorMessage = "Không thể thiếu mã câu lạc bộ")]
        public required Guid ClubId { get; set; }
        [Required(ErrorMessage = "Không thể thiếu mã khóa học")]
        public required Guid CourseId { get; set; }
        [Range(1, 50, ErrorMessage = "Tạo từ 1 tới 50 mã")]
        public int Quantity { get; set; }
    }
}
