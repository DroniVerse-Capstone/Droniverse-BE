using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace Droniverse.Identity.API;
// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {

        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không mong muốn tại {Path}. Error: {Error}", httpContext.Request.Path, ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }


    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var (statusCode, response) = exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                ErrorResponse.Create(ex.Message, ex.ErrorCode)
            ),
            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                ErrorResponse.Create(ex.Message, ex.ErrorCode)
            ),
            KeyNotFoundException ex => (
                StatusCodes.Status404NotFound,
                ErrorResponse.Create(ex.Message, "KEYNOTFOUND")
            ),
            DuplicateEmailException ex => (
                StatusCodes.Status409Conflict,
                ErrorResponse.Create(ex.Message, ex.ErrorCode)
            ),
            DbUpdateException ex => HandleDbUpdateException(ex),
            UnauthorizedAccessException ex => (
                StatusCodes.Status401Unauthorized,
                ErrorResponse.Create(ex.Message, "UNAUTHORIZED")
            ),
            ArgumentNullException ex => (
                StatusCodes.Status400BadRequest,
                ErrorResponse.Create(ex.Message, "ARGUMENT_NULL")
            ),
            InvalidOperationException ex => (
                StatusCodes.Status400BadRequest,
                ErrorResponse.Create(ex.Message, "INVALID_OPERATION")
            ),
            BadRequestException ex => (
                StatusCodes.Status400BadRequest,
                ErrorResponse.Create(ex.Message, "BAD_REQUEST")
            ),
            DomainException ex => (
                StatusCodes.Status400BadRequest,
                ErrorResponse.Create(ex.Message, ex.ErrorCode)
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                CreateInternalServerError(exception)
            )
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }

    private static (int StatusCode, ErrorResponse Response) HandleDbUpdateException(DbUpdateException ex)
    {
        if (ex.InnerException is MySqlException mySqlEx)
        {
            var (statusCode, message, errorCode) = mySqlEx.Number switch
            {
                // Lỗi khóa ngoại
                1451 or 1452 => (
                    StatusCodes.Status400BadRequest,
                    "Hành động này vi phạm khóa ngoại của cơ sở dữ liệu.",
                    "DB_FOREIGN_KEY_ERROR"
                ),

                //Lỗi trùng lặp
                1062 => (
                    StatusCodes.Status400BadRequest,
                    "Dữ liệu trùng lặp vi phạm chỉ mục duy nhất.",
                    "DB_DUPLICATE_KEY_ERROR"
                ),

                _ => (
                StatusCodes.Status500InternalServerError,
                "Lỗi thao tác với cơ sở dữ liệu.",
                "DB_ERROR"
                )
            };
            return (statusCode, ErrorResponse.Create(message, errorCode));
        }
        return (StatusCodes.Status500InternalServerError, ErrorResponse.Create("Lỗi cập nhật cơ sở dữ liệu", "DB_UPDATE_ERROR"));
    }
    private ErrorResponse CreateInternalServerError(Exception ex)
    {
        var message = _env.IsDevelopment()
            ? $"Internal Server Error: {ex.Message}"
            : "Đã xảy ra lỗi trong quá trình xử lý";
        return ErrorResponse.Create(message, "INTERNAL_ERROR");
    }
}


