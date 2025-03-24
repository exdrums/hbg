using API.Contacts.Model;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Service for managing users
/// </summary>
public interface IUserService
{
    Task<User> GetOrCreateUserFromOidcAsync(string oidcSubject, string name);
    Task<User> GetUserByIdAsync(string id);
    Task UpdateUserProfileAsync(string id, string name, string avatarUrl, string avatarAlt);
    Task UpdateUserPreferencesAsync(string id, UserPreferencesFlags preferences);
    Task<bool> IsUserOnlineAsync(string id);
}
