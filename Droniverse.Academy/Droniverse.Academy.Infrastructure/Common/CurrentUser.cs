using Droniverse.Shared.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Infrastructure.Common
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User =>
            _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated =>
            User?.Identity?.IsAuthenticated == true;

        public Guid UserId
        {
            get
            {
                if (!IsAuthenticated)
                    throw new UnauthorizedAccessException("User is not authenticated");

                var claim = User!.FindFirst("UserID");

                if (claim is null)
                    throw new UnauthorizedAccessException("UserID claim not found");

                if (!Guid.TryParse(claim.Value, out var userId))
                    throw new UnauthorizedAccessException("Invalid UserID claim format");

                return userId;
            }
        }
    }
}
