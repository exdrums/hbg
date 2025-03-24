using API.Contacts.Model;

namespace API.Contacts.Data.Repositories;

/// <summary>
/// Repository interface for Message entity
/// </summary>
public interface IMessageRepository
{
    Task<Message> GetByIdAsync(string id);
    Task<IEnumerable<Message>> GetByConversationIdAsync(string conversationId, int limit = 50, DateTime? before = null);
    Task<bool> AddAsync(Message message);
    Task<bool> UpdateAsync(Message message);
}
