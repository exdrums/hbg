using API.Contacts.Model;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Data.Repositories;

public class MessageRepository : RepositoryBase<Message>, IMessageRepository
{
    public MessageRepository(DbContext context) : base(context)
    {
    }

    public async Task<Message> GetByIdAsync(string id)
    {
        return await FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Message>> GetByConversationIdAsync(string conversationId, int limit = 50, DateTime? before = null)
    {
        var query = AsQueryable()
            .Where(m => m.ConversationId == conversationId);

        if (before.HasValue)
        {
            query = query.Where(m => m.Timestamp < before.Value);
        }

        return await query
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .OrderBy(m => m.Timestamp) // Re-order chronologically for the result
            .ToListAsync();
    }

    public async Task<bool> AddAsync(Message message)
    {
        await AddAsync(message, default);
        return await SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Message message)
    {
        Update(message);
        return await SaveChangesAsync();
    }
}