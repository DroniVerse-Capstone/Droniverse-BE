using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class UserRoundSubmitDto
    {

        [Required(ErrorMessage = "Thời gian thực thi là bắt buộc.")]
        public TimeSpan ExecutionTime { get; set; }

        [Required(ErrorMessage = "Điểm số là bắt buộc.")]
        public decimal Point { get; set; }

        [Required(ErrorMessage = "Trạng thái đậu là bắt buộc.")]
        public bool IsPassed { get; set; }
    }
}
