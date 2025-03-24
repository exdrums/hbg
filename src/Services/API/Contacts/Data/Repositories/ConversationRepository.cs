using API.Contacts.Model;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Data.Repositories;

public class ConversationRepository : RepositoryBase<Conversation>, IConversationRepository
{
    private readonly ChatDbContext _chatDbContext;

    public ConversationRepository(DbContext context) : base(context)
    {
        _chatDbContext = context as ChatDbContext;
    }

    public async Task<Conversation> GetByIdAsync(string id)
    {
        return await AsQueryable()
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Conversation>> GetByUserIdAsync(string userId, bool includeArchived = false)
    {
        var query = AsQueryable()
            .Include(c => c.Participants)
            .Where(c => c.Participants.Any(p => p.UserId == userId));

        if (!includeArchived)
        {
            query = query.Where(c => !c.IsArchived);
        }

        return await query
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();
    }

    public async Task<bool> AddAsync(Conversation conversation)
    {
        await AddAsync(conversation, default);
        return await SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Conversation conversation)
    {
        Update(conversation);
        return await SaveChangesAsync();
    }

    public async Task<bool> AddParticipantAsync(ConversationParticipant participant)
    {
        _chatDbContext.ConversationParticipants.Add(participant);
        return await SaveChangesAsync();
    }

    public async Task<bool> RemoveParticipantAsync(string conversationId, string userId)
    {
        var participant = await _chatDbContext.ConversationParticipants
            .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);

        if (participant != null)
        {
            _chatDbContext.ConversationParticipants.Remove(participant);
            return await SaveChangesAsync();
        }

        return false;
    }

    public async Task<bool> UpdateParticipantAsync(ConversationParticipant participant)
    {
        _chatDbContext.ConversationParticipants.Update(participant);
        return await SaveChangesAsync();
    }
}


