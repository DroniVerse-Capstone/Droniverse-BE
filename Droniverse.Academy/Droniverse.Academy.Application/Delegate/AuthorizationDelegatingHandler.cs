using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.Delegate
{
    public class AuthorizationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                // Lấy access token từ cookie
                var accessToken = httpContext.Request.Cookies["AccessToken"];

                if (string.IsNullOrEmpty(accessToken)) // Nếu cookie k có thì lấy từ header
                {
                    var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        accessToken = authHeader.Substring("Bearer ".Length).Trim();
                    }
                }

                //Forward token để Gateway
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
