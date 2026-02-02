namespace Droniverse.Shared.DTOs;

public class ResponseDto<T>
{
    public bool IsSuccess { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    // Constructor tiện lợi
    public void SetSuccess(T data, string msg = "Success")
    {
        IsSuccess = true;
        Data = data;
        Message = msg;
    }

    public void SetFailure(string errorMsg)
    {
        IsSuccess = false;
        Message = errorMsg;
    }
}
