namespace Droniverse.Shared.Authentication;

public class JwtSettings
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int AccessTokenExpirationDays { get; set; }
}

