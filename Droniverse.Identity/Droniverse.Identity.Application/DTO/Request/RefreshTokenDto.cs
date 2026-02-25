namespace Droniverse.Identity.Application.DTO.Request;
public record RefreshTokenDto(
    string RefreshToken,
    string AccessToken
)
{}

