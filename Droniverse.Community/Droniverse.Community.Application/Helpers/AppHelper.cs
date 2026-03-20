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
        public static string? GetFullName(UserResponse? user)
        {
            if (string.IsNullOrWhiteSpace(user?.FirstName) ||
                string.IsNullOrWhiteSpace(user?.LastName))
                return null;

            return $"{user.FirstName} {user.LastName}".Trim();
        }
    }
}
