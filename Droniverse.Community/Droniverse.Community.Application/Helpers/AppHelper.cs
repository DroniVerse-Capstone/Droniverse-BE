using Droniverse.Shared.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Helpers
{
    public static class AppHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        public static string? GetFullName(UserResponse? user)
        {
            if (string.IsNullOrWhiteSpace(user?.FirstName) ||
                string.IsNullOrWhiteSpace(user?.LastName))
                return null;

            return $"{user.FirstName} {user.LastName}".Trim();
        }
        public static DateTimeOffset ToVietnamTime(DateTime utcTime)
        {
            if (utcTime.Kind != DateTimeKind.Utc)
            {
                utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            }

            return TimeZoneInfo.ConvertTime(new DateTimeOffset(utcTime), VietnamTimeZone);
        }

        public static DateTimeOffset ToVietnamTime(DateTimeOffset dateTime)
        {
            return TimeZoneInfo.ConvertTime(dateTime, VietnamTimeZone);
        }

        public static DateTimeOffset GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, VietnamTimeZone);
        }
    }
}
