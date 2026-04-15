using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Request
{
    public class GenerateWithAssignCodeRequestDTO
    {
        [Required(ErrorMessage = "Không thể thiếu mã câu lạc bộ")]
        public required Guid ClubId { get; set; }
        [Required(ErrorMessage = "Không thể thiếu mã khóa học")]
        public required Guid CourseId { get; set; }
        [Range(1, 50, ErrorMessage = "Tạo từ 1 tới 50 mã")]
        public int Quantity { get; set; }
        public required string Email { get; set; }
        /// <summary>
        /// Lấy cả họ và tên
        /// </summary>
        public required string FullName { get; set; }
    }
}
