using API.Common;

namespace API.Emailer;

public class EmailerAppSettings : AppSettings
{
    public string DEFAULT_SENDER_ADDRESS { get; set; } = "";
    public string DEFAULT_SENDER_SERVER { get; set; } = "";
    public string DEFAULT_SENDER_PASSCODE { get; set; } = "";
}

public static class AppSettingsConfigiration {
    public static EmailerAppSettings ConfigureEmailerAppSettings(this IServiceCollection services, IConfiguration config) {
        services.AddOptions();
        EmailerAppSettings appSettings = new EmailerAppSettings();
        services.Configure<EmailerAppSettings>(config);
        config.Bind(appSettings);
        return appSettings;
    }
}
