using API.Identity.Shared.Configuration.Configuration.Identity;
using API.Identity.Configuration.Interfaces;

namespace API.Identity.Configuration;
public class RootConfiguration : IRootConfiguration
{
    public AdminConfiguration AdminConfiguration { get; } = new AdminConfiguration();
    public RegisterConfiguration RegisterConfiguration { get; } = new RegisterConfiguration();
}
