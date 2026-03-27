using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Shared.Helpers
{
    public static class AppHelper
    {
        public static string? GetFullName(UserResponse? user)
        {
            if (string.IsNullOrWhiteSpace(user?.FirstName) ||
                string.IsNullOrWhiteSpace(user?.LastName))
                return null;

            return $"{user.FirstName} {user.LastName}".Trim();
        }

        public static DateTime TrimToMinute(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
        }
    }
}
