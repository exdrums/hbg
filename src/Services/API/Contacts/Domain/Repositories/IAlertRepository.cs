using API.Contacts.Domain.Models;
using System.Threading.Tasks;

namespace API.Contacts.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Alert entities
    /// </summary>
    public interface IAlertRepository : IRepository<Alert>
    {
        /// <summary>
        /// Gets active alerts for a user
        /// </summary>
        Task<IEnumerable<Alert>> GetActiveForUserAsync(string userId, int limit = 50);

        /// <summary>
        /// Gets alerts for a user related to a specific conversation
        /// </summary>
        Task<IEnumerable<Alert>> GetForConversationAsync(string userId, string conversationId, int limit = 20);

        /// <summary>
        /// Marks an alert as read
        /// </summary>
        Task<bool> MarkAsReadAsync(string alertId);

        /// <summary>
        /// Marks all alerts as read for a user
        /// </summary>
        Task<int> MarkAllAsReadAsync(string userId);

        /// <summary>
        /// Dismisses an alert
        /// </summary>
        Task<bool> DismissAsync(string alertId);

        /// <summary>
        /// Dismisses all alerts for a user
        /// </summary>
        Task<int> DismissAllAsync(string userId);

        /// <summary>
        /// Dismisses alerts for a specific conversation
        /// </summary>
        Task<int> DismissForConversationAsync(string userId, string conversationId);

        /// <summary>
        /// Gets the count of unread alerts for a user
        /// </summary>
        Task<int> GetUnreadCountAsync(string userId);
    }
}
