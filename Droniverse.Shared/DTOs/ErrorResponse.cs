using System.Text.Json.Serialization;

namespace Droniverse.Shared.DTOs;

public class ErrorResponse : ApiResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ErrorCode { get; set; }
    private ErrorResponse() { } //private constructor to force use of factory method
    public static ErrorResponse Create(string message, string errorCode)
    {
        return new ErrorResponse
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode
        };
    }

}
