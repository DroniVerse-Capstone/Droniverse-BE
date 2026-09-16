namespace Droniverse.Identity.Application.DTO.Response;

/// <summary>
/// Response chứa service token được cấp bởi Identity service
/// </summary>
public class ServiceTokenResponse
{
    /// <summary>
    /// JWT token dùng cho service-to-service communication
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian token hết hạn (Unix timestamp)
    /// </summary>
    public long ExpiresIn { get; set; }

    /// <summary>
    /// Loại token (luôn là "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}
