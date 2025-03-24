using API.Contacts.Model;

namespace API.Contacts.Data.Repositories;

/// <summary>
/// Repository interface for Conversation entity
/// </summary>
public interface IConversationRepository
{
    Task<Conversation> GetByIdAsync(string id);
    Task<IEnumerable<Conversation>> GetByUserIdAsync(string userId, bool includeArchived = false);
    Task<bool> AddAsync(Conversation conversation);
    Task<bool> UpdateAsync(Conversation conversation);
    Task<bool> AddParticipantAsync(ConversationParticipant participant);
    Task<bool> RemoveParticipantAsync(string conversationId, string userId);
    Task<bool> UpdateParticipantAsync(ConversationParticipant participant);
}