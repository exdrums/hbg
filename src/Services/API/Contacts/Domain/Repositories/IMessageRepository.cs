using API.Contacts.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Message entities
    /// </summary>
    public interface IMessageRepository : IRepository<Message>
    {
        /// <summary>
        /// Gets messages for a conversation
        /// </summary>
        Task<IEnumerable<Message>> GetByConversationIdAsync(string conversationId, int limit = 50, DateTime? before = null);

        /// <summary>
        /// Gets messages for a conversation after a specific timestamp
        /// </summary>
        Task<IEnumerable<Message>> GetByConversationIdAfterTimestampAsync(string conversationId, DateTime timestamp);

        /// <summary>
        /// Gets unread messages for a user in a conversation
        /// </summary>
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(string conversationId, string userId, DateTime lastReadTimestamp);

        /// <summary>
        /// Gets the count of unread messages for a user in a conversation
        /// </summary>
        Task<int> GetUnreadCountAsync(string conversationId, string userId, DateTime lastReadTimestamp);

        /// <summary>
        /// Gets messages in a thread (replies to a specific message)
        /// </summary>
        Task<IEnumerable<Message>> GetThreadAsync(string parentMessageId, int limit = 50);

        /// <summary>
        /// Gets system alert messages for a conversation
        /// </summary>
        Task<IEnumerable<Message>> GetSystemAlertsAsync(string conversationId, int limit = 10);

        /// <summary>
        /// Gets a queryable source for messages
        /// </summary>
        IQueryable<Message> Query();

        /// <summary>
        /// Searches for messages containing a text query
        /// </summary>
        Task<IEnumerable<Message>> SearchTextAsync(string conversationId, string searchQuery, int limit = 20);
    }
}
