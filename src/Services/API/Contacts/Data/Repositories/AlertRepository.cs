using API.Contacts.Model;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Data.Repositories;

public class AlertRepository : RepositoryBase<Alert>, IAlertRepository
{
    public AlertRepository(DbContext context) : base(context)
    {
    }

    public async Task<Alert> GetByIdAsync(string id)
    {
        return await FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Alert>> GetActiveAlertsForUserAsync(string userId, string conversationId = null)
    {
        var now = DateTime.UtcNow;
        
        var query = AsQueryable()
            .Where(a => 
                (a.UserId == userId || a.UserId == null) && 
                (a.ExpiresAt == null || a.ExpiresAt > now));

        if (!string.IsNullOrEmpty(conversationId))
        {
            query = query.Where(a => a.ConversationId == conversationId || a.ConversationId == null);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> AddAsync(Alert alert)
    {
        await AddAsync(alert, default);
        return await SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var alert = await FirstOrDefaultAsync(a => a.Id == id);
        if (alert != null)
        {
            Remove(alert);
            return await SaveChangesAsync();
        }
        
        return false;
    }
}
