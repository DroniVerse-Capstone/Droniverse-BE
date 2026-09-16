namespace Droniverse.Community.Application.DTO.Response.Mongo;

public class MomoCreatePaymentResponse
{
    public string PayUrl { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string Message { get; set; } = string.Empty;
}

