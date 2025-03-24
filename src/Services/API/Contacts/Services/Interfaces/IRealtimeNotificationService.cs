using API.Contacts.Dtos;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Interface for real-time communication
/// </summary>
public interface IRealtimeNotificationService
{
    Task NotifyMessageReceived(string conversationId, MessageDto message);
    Task NotifyUserStartedTyping(string conversationId, UserDto user);
    Task NotifyUserStoppedTyping(string conversationId, UserDto user);
    Task NotifyAlertsChanged(string userId, IEnumerable<AlertDto> alerts);
    Task SubscribeToConversation(string connectionId, string conversationId);
    Task UnsubscribeFromConversation(string connectionId, string conversationId);
}
