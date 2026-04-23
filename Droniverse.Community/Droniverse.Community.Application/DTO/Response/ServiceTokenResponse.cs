namespace Droniverse.Community.Application.DTO.Response;

/// <summary>
/// DTO chứa service token response từ Identity service
/// </summary>
public class ServiceTokenResponse
{
    /// <summary>
    /// JWT token dùng cho service-to-service communication
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian hết hạn (Unix timestamp)
    /// </summary>
    public long ExpiresIn { get; set; }

    /// <summary>
    /// Loại token
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}
