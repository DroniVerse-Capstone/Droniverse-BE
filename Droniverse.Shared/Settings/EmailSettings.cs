namespace Droniverse.Shared.Settings;

public class EmailSettings
{
    public string Mail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}

