using API.Contacts.Model;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Service for managing alerts
/// </summary>
public interface IAlertService
{
    Task<Alert> CreateAlertAsync(string message, string userId = null, string conversationId = null, TimeSpan? expiration = null);
    Task DeleteAlertAsync(string id);
    Task<IEnumerable<Alert>> GetActiveAlertsForUserAsync(string userId, string conversationId = null);
}
