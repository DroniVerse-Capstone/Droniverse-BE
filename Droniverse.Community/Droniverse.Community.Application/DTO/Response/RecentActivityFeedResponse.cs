namespace Droniverse.Community.Application.DTO.Response
{
    public class RecentActivityFeedResponse
    {
        public List<ActivityFeedItem> Activities { get; set; } = new();
    }

    public class ActivityFeedItem
    {
        public string ActivityType { get; set; } = string.Empty; // e.g., "COURSE_COMPLETED", "CLUB_CREATED", "NEW_TRANSACTION"
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
