using API.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Identity.Admin.Spa.Controllers;

[Route("[controller]")]
[ApiController]
public class ConfigurationController : Controller
{
    private readonly IOptionsSnapshot<AppSettings> settings;
    public ConfigurationController(IOptionsSnapshot<AppSettings> settings)
    {
        this.settings = settings;
    }
    public IActionResult Index()
    {
        return Json(new ClientAppSettings(this.settings.Value));
    }
}

public class ClientAppSettings
{
    public ClientAppSettings(AppSettings settings)
    {
        HBGIDENTITYADMINSPA = settings.HBGIDENTITYADMINSPA;
        HBGIDENTITYADMINSPADEV = settings.HBGIDENTITYADMINSPADEV;
        HBGIDENTITY = settings.HBGIDENTITY;
        HBGIDENTITYADMINAPI = settings.HBGIDENTITYADMINAPI;
    }
    public string HBGIDENTITYADMINSPA { get; set; } = "";
    public string HBGIDENTITYADMINSPADEV { get; set; } = "";
    public string HBGIDENTITY { get; set; } = "";
    public string HBGIDENTITYADMINAPI { get; set; } = "";
}
