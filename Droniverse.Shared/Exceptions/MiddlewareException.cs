using Microsoft.AspNetCore.Builder;

namespace Droniverse.Shared.Exceptions;

public static class MiddlewareException
{
    public static IApplicationBuilder UseGlobalExceptionHandlerMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }

}
