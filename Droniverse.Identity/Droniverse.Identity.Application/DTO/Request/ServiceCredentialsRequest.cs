namespace Droniverse.Identity.Application.DTO.Request;

/// <summary>
/// Request để lấy Service-to-Service token từ Identity service
/// </summary>
public class ServiceCredentialsRequest
{
    /// <summary>
    /// ID của service đang yêu cầu token (vd: "community-service")
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// API Key bí mật của service (được lưu trong environment variable)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
