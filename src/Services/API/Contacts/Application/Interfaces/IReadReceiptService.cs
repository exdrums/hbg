using System;
using System.Threading.Tasks;

namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Interface for managing message read receipts
    /// </summary>
    public interface IReadReceiptService
    {
        /// <summary>
        /// Marks messages as read by a user up to a specific timestamp
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="timestamp">Timestamp up to which to mark messages as read</param>
        /// <returns>Number of messages marked as read</returns>
        Task<int> MarkMessagesAsReadAsync(string conversationId, string userId, DateTime timestamp);

        /// <summary>
        /// Gets the timestamp of when a user last read a conversation
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="conversationId">Conversation ID</param>
        /// <returns>Timestamp when the user last read the conversation</returns>
        Task<DateTime> GetLastReadTimestampAsync(string userId, string conversationId);

        /// <summary>
        /// Gets all read receipts for a conversation
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <returns>Dictionary mapping user IDs to their last read timestamp</returns>
        Task<IDictionary<string, DateTime>> GetReadReceiptsForConversationAsync(string conversationId);

        /// <summary>
        /// Gets the count of unread messages for a user in a conversation
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>Count of unread messages</returns>
        Task<int> GetUnreadMessageCountAsync(string conversationId, string userId);
    }
}
