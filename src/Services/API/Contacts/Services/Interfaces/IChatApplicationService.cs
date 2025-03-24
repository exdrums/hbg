using API.Contacts.Model;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Application service for chat operations
/// </summary>
public interface IChatApplicationService
{
    // User-to-User Chat Operations
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(string userId);
    Task<Conversation> CreateConversationAsync(string creatorId, IEnumerable<string> participantIds, string title);
    Task<Message> SendMessageAsync(string conversationId, string userId, string text, string parentMessageId = null);
    Task<IEnumerable<Message>> GetConversationMessagesAsync(string conversationId, string userId, int limit = 50);
    Task UserStartedTypingAsync(string conversationId, string userId);
    Task UserStoppedTypingAsync(string conversationId, string userId);

    // AI Assistant Chat Operations
    Task<Conversation> CreateAiAssistantConversationAsync(string userId, string title = "AI Assistant");
    Task<Message> SendMessageToAiAsync(string conversationId, string userId, string text);
    Task<Message> RegenerateAiResponseAsync(string conversationId, string messageId, string userId);
    Task<IEnumerable<Alert>> GetAlertsAsync(string userId, string conversationId);
}
