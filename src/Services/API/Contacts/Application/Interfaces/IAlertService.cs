using API.Contacts.Application.Dtos;
using API.Contacts.Domain.Models;
using System.Threading.Tasks;

namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Application service interface for alert operations
    /// </summary>
    public interface IAlertService
    {
        /// <summary>
        /// Gets active alerts for a user
        /// </summary>
        Task<IEnumerable<AlertDto>> GetActiveAlertsForUserAsync(string userId, int limit = 50);

        /// <summary>
        /// Gets alerts for a user related to a specific conversation
        /// </summary>
        Task<IEnumerable<AlertDto>> GetAlertsForConversationAsync(string userId, string conversationId, int limit = 20);

        /// <summary>
        /// Creates a new alert
        /// </summary>
        Task<AlertDto> CreateAlertAsync(string userId, AlertType type, string text, string conversationId = null, string messageId = null);

        /// <summary>
        /// Creates a new message alert
        /// </summary>
        Task<AlertDto> CreateMessageAlertAsync(string userId, string conversationId, string messageId, string authorName);

        /// <summary>
        /// Creates a new system alert
        /// </summary>
        Task<AlertDto> CreateSystemAlertAsync(string userId, string text);

        /// <summary>
        /// Marks an alert as read
        /// </summary>
        Task MarkAsReadAsync(string alertId, string userId);

        /// <summary>
        /// Marks all alerts as read for a user
        /// </summary>
        Task<int> MarkAllAsReadAsync(string userId);

        /// <summary>
        /// Dismisses an alert
        /// </summary>
        Task DismissAlertAsync(string alertId, string userId);

        /// <summary>
        /// Dismisses all alerts for a user
        /// </summary>
        Task<int> DismissAllAlertsAsync(string userId);

        /// <summary>
        /// Dismisses alerts for a specific conversation
        /// </summary>
        Task<int> DismissAlertsForConversationAsync(string userId, string conversationId);

        /// <summary>
        /// Gets the count of unread alerts for a user
        /// </summary>
        Task<int> GetUnreadAlertCountAsync(string userId);
    }
}
