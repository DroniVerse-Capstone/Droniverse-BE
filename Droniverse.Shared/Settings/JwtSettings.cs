namespace Droniverse.Shared.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 86400;
    public int RefreshTokenExpirationDays { get; set; } = 60;
}

