using API.Contacts.Model;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Service for managing conversations
/// </summary>
public interface IConversationService
{
    Task<Conversation> CreateUserToUserConversationAsync(string creatorId, IEnumerable<string> participantIds, string title = null);
    Task<Conversation> CreateAiAssistantConversationAsync(string userId, string title = null);
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(string userId, bool includeArchived = false);
    Task<Conversation> GetConversationByIdAsync(string id);
    Task AddUserToConversationAsync(string conversationId, string userId, ParticipantRole role = ParticipantRole.User);
    Task RemoveUserFromConversationAsync(string conversationId, string userId);
    Task ArchiveConversationAsync(string conversationId, string userId);
    Task RestoreConversationAsync(string conversationId, string userId);
    Task<bool> IsUserInConversationAsync(string conversationId, string userId);
}
