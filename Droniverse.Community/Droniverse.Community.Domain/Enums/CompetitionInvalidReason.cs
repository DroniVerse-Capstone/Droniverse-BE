using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Enums
{
    public enum CompetitionInvalidReason
    {
        /// <summary>
        /// Không có round hợp lệ (Pending) để tổ chức thi đấu.
        /// Thường xảy ra khi chưa tạo round hoặc tất cả round đều bị invalid.
        /// </summary>
        NoRounds,

        /// <summary>
        /// Không có người tham gia cuộc thi.
        /// Không thể bắt đầu cuộc thi khi danh sách participant rỗng.
        /// </summary>
        NoParticipants,

        /// <summary>
        /// Lịch thi đấu không hợp lệ.
        /// Ví dụ: thời gian của round nằm ngoài khoảng StartDate - EndDate của competition.
        /// </summary>
        ScheduleInvalid,

        /// <summary>
        /// Thời gian đăng ký không hợp lệ.
        /// Ví dụ: RegistrationStartDate >= RegistrationEndDate hoặc conflict với timeline khác.
        /// </summary>
        RegistrationTimeInvalid,

        /// <summary>
        /// Thời gian diễn ra cuộc thi không hợp lệ.
        /// Ví dụ: StartDate >= EndDate hoặc không phù hợp với registration timeline.
        /// </summary>
        CompetitionTimeInvalid,

        /// <summary>
        /// Thất bại khi chuyển trạng thái sang ONGOING.
        /// Có thể do vi phạm rule domain (thiếu round, thiếu participant, hoặc logic khác).
        /// </summary>
        StartFailed,

        /// <summary>
        /// Thất bại khi kết thúc cuộc thi.
        /// Ví dụ: dữ liệu chưa hoàn chỉnh hoặc lỗi trong quá trình finalize.
        /// </summary>
        FinishFailed,

        /// <summary>
        /// Lỗi hệ thống không xác định.
        /// Dùng khi exception không thuộc các trường hợp cụ thể.
        /// </summary>
        SystemError,

        /// <summary>
        /// Lỗi do dependency bên ngoài.
        /// Ví dụ: gọi API khác (Academy, Email, Payment...) bị fail.
        /// </summary>
        DependencyFailed,

        /// <summary>
        /// Không xác định được nguyên nhân cụ thể.
        /// Chỉ nên dùng khi không thể phân loại lỗi.
        /// </summary>
        Unknown
    }
}
