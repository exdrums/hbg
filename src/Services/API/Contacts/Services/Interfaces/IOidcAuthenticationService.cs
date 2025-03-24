namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Interface for OIDC authentication
/// </summary>
public interface IOidcAuthenticationService
{
    Task<(string subject, string name)> ValidateTokenAsync(string token);
    Task<bool> IsUserAuthorizedForConversationAsync(string userId, string conversationId);
}
