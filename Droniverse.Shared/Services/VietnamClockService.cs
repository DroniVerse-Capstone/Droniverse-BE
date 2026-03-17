namespace Droniverse.Shared.Services;

public class ClockService : IClock
{
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                return TimeZoneInfo.CreateCustomTimeZone(
                    id: "Vietnam Standard Time",
                    baseUtcOffset: TimeSpan.FromHours(7),
                    displayName: "(UTC+07:00) Vietnam",
                    standardDisplayName: "Vietnam Standard Time");
            }
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone(
                id: "Vietnam Standard Time",
                baseUtcOffset: TimeSpan.FromHours(7),
                displayName: "(UTC+07:00) Vietnam",
                standardDisplayName: "Vietnam Standard Time");
        }
    }
}
