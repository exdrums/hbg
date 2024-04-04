namespace API.Common
{
    public class AppSettings
    {
        public string HBGDB { get; set; } = "";
        public string HBGPROJECTSDB { get; set; } = "";
        public string HBGSPA { get; set; } = "";
        public string HBGSPADEV { get; set; } = "";
        public string HBGIDENTITY { get; set; } = "";
        public string HBGIDENTITYADMIN { get; set; } = "";
        public string HBGFILES { get; set; } = "";
        public string HBGPROJECTS { get; set; } = "";
        
        public string BASEDIR { get; set; } = "";
        public string AUDIENCE { get; set; } = "";

        public bool? EnableSeeding { get; set; } = false;
    }

    public static class AppSettingsConfigiration {
        public static AppSettings ConfigureAppSettings(this IServiceCollection services, IConfiguration config) {
            services.AddOptions();
            AppSettings appSettings = new AppSettings();
            services.Configure<AppSettings>(config);
            config.Bind(appSettings);
            return appSettings;
        }
    }
}