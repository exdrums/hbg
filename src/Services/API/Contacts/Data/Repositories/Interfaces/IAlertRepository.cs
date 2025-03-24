using API.Contacts.Model;

namespace API.Contacts.Data.Repositories;

/// <summary>
/// Repository interface for Alert entity
/// </summary>
public interface IAlertRepository
{
    Task<Alert> GetByIdAsync(string id);
    Task<IEnumerable<Alert>> GetActiveAlertsForUserAsync(string userId, string conversationId = null);
    Task<bool> AddAsync(Alert alert);
    Task<bool> DeleteAsync(string id);
}
