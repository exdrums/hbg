namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Manager for SignalR connection mappings
/// </summary>
public interface IConnectionManager
{
    void AddConnection(string connectionId, string userId);
    void RemoveConnection(string connectionId);
    void AddToConversation(string connectionId, string conversationId);
    void RemoveFromConversation(string connectionId, string conversationId);
    IEnumerable<string> GetConnectionsForUser(string userId);
    IEnumerable<string> GetConnectionsForConversation(string conversationId);
}
