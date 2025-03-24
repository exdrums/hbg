using API.Contacts.Model;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Service for managing messages
/// </summary>
public interface IMessageService
{
    Task<Message> SendMessageAsync(string conversationId, string userId, string text, string parentMessageId = null);
    Task<Message> EditMessageAsync(string messageId, string userId, string newText);
    Task<IEnumerable<Message>> GetConversationMessagesAsync(string conversationId, int limit = 50, DateTime? before = null);
    Task<Message> RegenerateAiMessageAsync(string messageId, string userId);
    Task<Message> CreateSystemAlertMessageAsync(string conversationId, string alertText);
    Task MarkMessagesAsReadAsync(string conversationId, string userId, DateTime timestamp);
}
