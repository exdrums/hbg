using API.Contacts.Model;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Service for integrating with AI providers
/// </summary>
public interface IAiAssistantService
{
    Task<string> GetAiResponseAsync(IEnumerable<Message> conversationHistory, string userId);
    Task<string> RegenerateResponseAsync(IEnumerable<Message> conversationHistory, string messageId);
    Task<bool> IsRequestLimitReachedAsync(string userId);
}
