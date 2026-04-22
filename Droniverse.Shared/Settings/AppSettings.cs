namespace Droniverse.Shared.Settings;

public class AppSettings
{
    public const string SectionName = "AppSettings";
    public string FrontendUrl { get; set; } = string.Empty;
    public string BackendUrl { get; set; } = string.Empty;
}
