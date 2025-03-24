namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Interface for managing message read receipts.
/// </summary>
public interface IReadReceiptService
{
    /// <summary>
    /// Marks all messages in a conversation as read by a user up to a specific timestamp.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="timestamp">Timestamp up to which to mark messages as read</param>
    /// <returns>Number of messages marked as read</returns>
    Task<int> MarkMessagesAsReadAsync(string conversationId, string userId, DateTime timestamp);
    
    /// <summary>
    /// Gets the read receipt timestamp for a user in a conversation.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="conversationId">Conversation ID</param>
    /// <returns>Timestamp when the user last read the conversation</returns>
    DateTime GetReadReceipt(string userId, string conversationId);
    
    /// <summary>
    /// Gets all read receipts for a conversation.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <returns>Dictionary mapping user IDs to their last read timestamp</returns>
    Task<IDictionary<string, DateTime>> GetReadReceiptsForConversationAsync(string conversationId);
}