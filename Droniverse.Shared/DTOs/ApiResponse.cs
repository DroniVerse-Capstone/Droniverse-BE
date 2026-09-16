namespace Droniverse.Shared.DTOs;

public abstract class ApiResponse
{
    public bool IsSuccess { get; set; } = true;
    public string Message { get; set; } = string.Empty;
}
