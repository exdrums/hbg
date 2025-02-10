using API.Identity.Shared.Configuration.Configuration.Identity;

namespace API.Identity.Configuration.Interfaces;
public interface IRootConfiguration
{
    AdminConfiguration AdminConfiguration { get; }

    RegisterConfiguration RegisterConfiguration { get; }
}
