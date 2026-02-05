using System.Text.Json.Serialization;

namespace Droniverse.Shared.DTOs;

public class SuccessResponse<T> : ApiResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T Data { get; set; }
    private SuccessResponse() { } //private constructor to force use of factory method
    public static SuccessResponse<T> Create(T data, string message = "")
    {
        return new SuccessResponse<T>
        {
            IsSuccess = true,
            Message = message ?? "Sucessfully",
            Data = data
        };
    }
}
