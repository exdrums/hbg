using API.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Web.SPA.Controllers;

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
        HBGSPA = settings.HBGSPA;
        HBGSPADEV = settings.HBGSPADEV;
        HBGIDENTITY = settings.HBGIDENTITY;
        HBGFILES = settings.HBGFILES;
        HBGPROJECTS = settings.HBGPROJECTS;
    }
    public string HBGSPA { get; set; } = "";
    public string HBGSPADEV { get; set; } = "";
    public string HBGIDENTITY { get; set; } = "";
    // public string HBGIDENTITYADMIN { get; set; } = "";
    public string HBGFILES { get; set; } = "";
    public string HBGPROJECTS { get; set; } = "";

}

