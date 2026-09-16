namespace Droniverse.Community.Domain.Enums
{
    /// <summary>
    /// Trạng thái tiến trình của người chơi trong vòng thi
    /// </summary>
    public enum UserRoundStatus
    {
        /// <summary>
        /// Người chơi đang tham gia vòng thi
        /// </summary>
        InProgress = 0,

        /// <summary>
        /// Người chơi đã hoàn thành vòng thi
        /// </summary>
        Completed = 1,

        /// <summary>
        /// Người chơi bị loại hoặc hủy kết quả
        /// </summary>
        Disqualified = 2
    }
}
