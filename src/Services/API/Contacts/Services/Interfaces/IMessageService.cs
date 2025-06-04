using API.Contacts.Models;

namespace API.Contacts.Services.Interfaces;
/// <summary>
/// Defines the contract for message-related business logic
/// 
/// Design Philosophy:
/// Messages are the atomic units of communication in our chat system.
/// This service treats messages as immutable after creation (except
/// for specific operations like edit/delete), which simplifies
/// concurrent access and maintains chat history integrity.
/// 
/// Key architectural decisions:
/// 1. Messages are never truly deleted - only marked as deleted
/// 2. Edit history could be maintained (not implemented here)
/// 3. Read receipts are part of the message, not separate entities
/// 4. Messages know their conversation (aggregate relationship)
/// 
/// This design supports requirements like message recovery, audit
/// trails, and compliance with data retention policies.
/// </summary>
public interface IMessageService
{
    #region Query Operations

    /// <summary>
    /// Retrieves all messages for a specific conversation
    /// This is the primary method for loading chat history
    /// 
    /// Performance considerations:
    /// - Should support pagination (via IQueryable)
    /// - Should order by SentAt timestamp (oldest first for correct display)
    /// - Should include deleted messages (shown as "deleted")
    /// - Could implement lazy loading of message metadata
    /// 
    /// The IQueryable return type allows:
    /// - Efficient pagination with Skip/Take
    /// - Additional filtering (e.g., by date range)
    /// - Integration with DevExtreme DataSource
    /// </summary>
    /// <param name="conversationId">The conversation to load messages from</param>
    /// <returns>Queryable collection of messages</returns>
    Task<IQueryable<Message>> GetMessagesAsync(Guid conversationId);

    /// <summary>
    /// Retrieves messages for a conversation within a specific time range
    /// Useful for loading messages around a specific point (jump to date)
    /// 
    /// Use cases:
    /// - "Show messages from today"
    /// - Loading context around search results  
    /// - Implementing message archival by date
    /// </summary>
    /// <param name="conversationId">The conversation to query</param>
    /// <param name="startDate">Start of the time range (inclusive)</param>
    /// <param name="endDate">End of the time range (inclusive)</param>
    /// <returns>Messages within the specified range</returns>
    Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(
        Guid conversationId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Retrieves a single message by ID
    /// Used for operations on specific messages
    /// 
    /// Common uses:
    /// - Loading a message for editing
    /// - Validating reply-to relationships
    /// - Displaying message details/info
    /// </summary>
    /// <param name="messageId">The message to retrieve</param>
    /// <returns>The message or null if not found</returns>
    Task<Message> GetMessageAsync(Guid messageId);

    /// <summary>
    /// Searches for messages containing specific text
    /// Enables the search functionality in chat
    /// 
    /// Search behavior:
    /// - Case-insensitive search in message content
    /// - Should exclude deleted message content
    /// - Could be extended to search metadata
    /// - Results ordered by relevance or date
    /// 
    /// Future enhancements:
    /// - Full-text search with relevance ranking
    /// - Search in attachments
    /// - Regular expression support
    /// </summary>
    /// <param name="conversationId">Conversation to search in (null for all)</param>
    /// <param name="searchText">Text to search for</param>
    /// <param name="userId">User performing the search (for permissions)</param>
    /// <returns>Messages matching the search criteria</returns>
    Task<IEnumerable<Message>> SearchMessagesAsync(
        Guid? conversationId,
        string searchText,
        string userId);

    /// <summary>
    /// Gets unread messages for a user across all conversations
    /// Used for notification badges and summaries
    /// 
    /// This enables features like:
    /// - Unread count badges
    /// - "Mark all as read" functionality  
    /// - Notification summaries
    /// </summary>
    /// <param name="userId">The user to check unread messages for</param>
    /// <returns>Collection of unread messages</returns>
    Task<IEnumerable<Message>> GetUnreadMessagesAsync(string userId);

    #endregion

    #region Command Operations

    /// <summary>
    /// Saves a new message to the data store
    /// This is called after message creation and validation
    /// 
    /// The save operation should:
    /// 1. Persist to the database
    /// 2. Update any cache layers
    /// 3. Trigger any post-save events
    /// 4. Be idempotent (safe to retry)
    /// 
    /// Transaction considerations:
    /// - Should be atomic with conversation updates
    /// - Should handle concurrent saves gracefully
    /// </summary>
    /// <param name="message">The message to save</param>
    /// <returns>The saved message (may include generated fields)</returns>
    Task<Message> SaveMessageAsync(Message message);

    /// <summary>
    /// Updates an existing message
    /// Used for edits, deletes, and read receipt updates
    /// 
    /// Update rules:
    /// - Only specific fields can be updated
    /// - Must maintain audit trail
    /// - Should version messages if edit history is needed
    /// 
    /// This method is intentionally generic to support
    /// various update scenarios while maintaining encapsulation
    /// </summary>
    /// <param name="message">The message with updated values</param>
    /// <returns>The updated message</returns>
    Task<Message> UpdateMessageAsync(Message message);

    /// <summary>
    /// Marks all messages in a conversation as read by a user
    /// Bulk operation for efficiency
    /// 
    /// Common scenario:
    /// - User opens a conversation with many unread messages
    /// - All visible messages should be marked read
    /// - More efficient than individual updates
    /// </summary>
    /// <param name="conversationId">The conversation</param>
    /// <param name="userId">The user who read the messages</param>
    /// <returns>Number of messages marked as read</returns>
    Task<int> MarkConversationAsReadAsync(Guid conversationId, string userId);

    /// <summary>
    /// Deletes messages older than a specified date
    /// Used for data retention policies and cleanup
    /// 
    /// Retention considerations:
    /// - May need to preserve messages for legal reasons
    /// - Should respect conversation-specific retention settings
    /// - Could move to cold storage instead of deleting
    /// 
    /// This is typically a scheduled background operation
    /// </summary>
    /// <param name="conversationId">The conversation to clean up</param>
    /// <param name="beforeDate">Delete messages before this date</param>
    /// <returns>Number of messages deleted</returns>
    Task<int> DeleteOldMessagesAsync(Guid conversationId, DateTime beforeDate);

    #endregion

    #region Analytics and Reporting

    /// <summary>
    /// Gets message statistics for a conversation
    /// Useful for analytics and moderation
    /// 
    /// Statistics might include:
    /// - Total message count
    /// - Messages per user
    /// - Average message length
    /// - Most active times
    /// - Media vs text ratio
    /// </summary>
    /// <param name="conversationId">The conversation to analyze</param>
    /// <returns>Statistics object with various metrics</returns>
    Task<MessageStatistics> GetMessageStatisticsAsync(Guid conversationId);

    #endregion
}

/// <summary>
/// Data transfer object for message statistics
/// Demonstrates the DTO pattern for returning complex data
/// </summary>
public class MessageStatistics
{
    /// <summary>
    /// Total number of messages in the conversation
    /// </summary>
    public int TotalMessages { get; set; }
    
    /// <summary>
    /// Breakdown of messages by user
    /// Key: UserId, Value: Message count
    /// </summary>
    public Dictionary<string, int> MessagesByUser { get; set; }
    
    /// <summary>
    /// Average message length in characters
    /// </summary>
    public double AverageMessageLength { get; set; }
    
    /// <summary>
    /// Timestamp of the first message
    /// </summary>
    public DateTime FirstMessageAt { get; set; }
    
    /// <summary>
    /// Timestamp of the most recent message
    /// </summary>
    public DateTime LastMessageAt { get; set; }
    
    /// <summary>
    /// Breakdown by message type
    /// </summary>
    public Dictionary<string, int> MessagesByType { get; set; }
}
