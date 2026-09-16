using System.Text.Json.Serialization;

namespace Droniverse.Shared.DTOs;

public class SuccessResponse<T> : ApiResponse
{
    public T Data { get; set; }
    [JsonConstructor]
    public SuccessResponse() { } //private constructor to force use of factory method
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
