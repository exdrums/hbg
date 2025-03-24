using API.Contacts.Data.Repositories;
using API.Contacts.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Services;

/// <summary>
/// Service for managing message read receipts.
/// </summary>
public class ReadReceiptService : IReadReceiptService
{
    private readonly MessageRepository _messageRepository;
    private readonly UserRepository _userRepository;
    private readonly IRealtimeNotificationService _notificationService;
    private readonly ILogger<ReadReceiptService> _logger;
    
    // In-memory cache of read receipts (userId -> conversationId -> timestamp)
    // In a production environment, this would be stored in a persistent database
    private readonly Dictionary<string, Dictionary<string, DateTime>> _readReceipts = new();

    public ReadReceiptService(
        MessageRepository messageRepository,
        UserRepository userRepository,
        IRealtimeNotificationService notificationService,
        ILogger<ReadReceiptService> logger)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Marks all messages in a conversation as read by a user up to a specific timestamp.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="timestamp">Timestamp up to which to mark messages as read</param>
    /// <returns>Number of messages marked as read</returns>
    public async Task<int> MarkMessagesAsReadAsync(string conversationId, string userId, DateTime timestamp)
    {
        try
        {
            // Verify the user exists
            var user = await _userRepository.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User {userId} not found");
            }
            
            // Get previously read timestamp
            var previousReadTime = GetReadReceipt(userId, conversationId);
            
            // Count messages that were previously unread
            var unreadMessagesCount = await _messageRepository
                .Query()
                .CountAsync(m => 
                    m.ConversationId == conversationId &&
                    m.Author.Id != userId && // Don't count own messages
                    m.Timestamp > previousReadTime &&
                    m.Timestamp <= timestamp
                );
            
            // Update read receipt
            SetReadReceipt(userId, conversationId, timestamp);
            
            // Notify other participants about the read status update
            await NotifyReadReceiptUpdatedAsync(conversationId, userId, timestamp);
            
            _logger.LogInformation("User {UserId} marked {Count} messages as read in conversation {ConversationId}", 
                userId, unreadMessagesCount, conversationId);
            
            return unreadMessagesCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read in conversation {ConversationId} by user {UserId}", 
                conversationId, userId);
            throw;
        }
    }

    /// <summary>
    /// Gets the read receipt timestamp for a user in a conversation.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="conversationId">Conversation ID</param>
    /// <returns>Timestamp when the user last read the conversation</returns>
    public DateTime GetReadReceipt(string userId, string conversationId)
    {
        // Check if we have read receipts for this user
        if (!_readReceipts.TryGetValue(userId, out var userReceipts))
        {
            return DateTime.MinValue;
        }
        
        // Check if we have a read receipt for this conversation
        if (!userReceipts.TryGetValue(conversationId, out var timestamp))
        {
            return DateTime.MinValue;
        }
        
        return timestamp;
    }

    /// <summary>
    /// Gets all read receipts for a conversation.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <returns>Dictionary mapping user IDs to their last read timestamp</returns>
    public async Task<IDictionary<string, DateTime>> GetReadReceiptsForConversationAsync(string conversationId)
    {
        var result = new Dictionary<string, DateTime>();
        
        try
        {
            // Get all users in the conversation
            var connectedUserIds = await _notificationService.GetConnectedUserIdsAsync(conversationId);
            
            // Get read receipts for all users
            foreach (var userId in connectedUserIds)
            {
                var timestamp = GetReadReceipt(userId, conversationId);
                result[userId] = timestamp;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting read receipts for conversation {ConversationId}", conversationId);
        }
        
        return result;
    }

    /// <summary>
    /// Sets a read receipt timestamp for a user in a conversation.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="timestamp">Timestamp</param>
    private void SetReadReceipt(string userId, string conversationId, DateTime timestamp)
    {
        // Ensure we have a dictionary for this user
        if (!_readReceipts.TryGetValue(userId, out var userReceipts))
        {
            userReceipts = new Dictionary<string, DateTime>();
            _readReceipts[userId] = userReceipts;
        }
        
        // Update the timestamp
        userReceipts[conversationId] = timestamp;
    }

    /// <summary>
    /// Notifies other users in a conversation that a user has read messages.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="timestamp">Read timestamp</param>
    private async Task NotifyReadReceiptUpdatedAsync(string conversationId, string userId, DateTime timestamp)
    {
        try
        {
            // Get all other users in the conversation
            var connectedUserIds = await _notificationService.GetConnectedUserIdsAsync(conversationId);
            
            // Notify each user individually (except the one who just read)
            foreach (var connectedUserId in connectedUserIds)
            {
                if (connectedUserId != userId)
                {
                    // In a real implementation, this would use a SignalR notification
                    // For this sample, we'll just log it
                    _logger.LogDebug("Notifying user {ConnectedUserId} that user {UserId} read messages in conversation {ConversationId}", 
                        connectedUserId, userId, conversationId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying read receipt update for user {UserId} in conversation {ConversationId}", 
                userId, conversationId);
        }
    }
}