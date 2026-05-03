using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Request
{
    /// <summary>
    /// Filter request cho API lấy thống kê cuộc thi.
    /// </summary>
    public class CompetitionFilterRequest
    {
        /// <summary>Lọc theo trạng thái cuộc thi.</summary>
        public CompetitionStatus? CompetitionStatus { get; set; }

        /// <summary>Lọc theo giai đoạn cuộc thi (lifecycle phase).</summary>
        public CompetitionLifeCycleStatus? CompetitionPhase { get; set; }

        /// <summary>Lọc theo ID câu lạc bộ (chỉ dành cho API admin).</summary>
        public Guid? ClubId { get; set; }

        /// <summary>Lọc cuộc thi có ngày bắt đầu từ ngày này trở đi.</summary>
        public DateTime? StartDateFrom { get; set; }

        /// <summary>Lọc cuộc thi có ngày bắt đầu đến ngày này.</summary>
        public DateTime? StartDateTo { get; set; }

        /// <summary>Lọc cuộc thi có ngày kết thúc từ ngày này trở đi.</summary>
        public DateTime? EndDateFrom { get; set; }

        /// <summary>Lọc cuộc thi có ngày kết thúc đến ngày này.</summary>
        public DateTime? EndDateTo { get; set; }

        /// <summary>Lọc cuộc thi được tạo bởi user ID này.</summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>Lọc cuộc thi được cập nhật bởi user ID này.</summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>Lọc cuộc thi có số vòng thi tối thiểu.</summary>
        public int? MinTotalRounds { get; set; }

        /// <summary>Lọc cuộc thi có số vòng thi tối đa.</summary>
        public int? MaxTotalRounds { get; set; }

        /// <summary>Lọc cuộc thi có số giải thưởng tối thiểu.</summary>
        public int? MinTotalPrizes { get; set; }

        /// <summary>Lọc cuộc thi có số giải thưởng tối đa.</summary>
        public int? MaxTotalPrizes { get; set; }

        /// <summary>Lọc cuộc thi có số thí sinh tối thiểu.</summary>
        public int? MinTotalCompetitors { get; set; }

        /// <summary>Lọc cuộc thi có số thí sinh tối đa.</summary>
        public int? MaxTotalCompetitors { get; set; }
    }
}
