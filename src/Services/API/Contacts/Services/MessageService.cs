using API.Contacts.Data;
using API.Contacts.Models;
using API.Contacts.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Services
{
    /// <summary>
    /// Implementation of message-related business logic
    /// 
    /// Performance Considerations:
    /// 1. Messages are the highest-volume entity in chat systems
    /// 2. Read operations far exceed write operations
    /// 3. Most queries are by conversation ID with date ordering
    /// 4. Consider caching strategies for hot conversations
    /// 
    /// This service handles the core messaging functionality while
    /// maintaining message immutability and audit trails
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<MessageService> _logger;

        public MessageService(ChatDbContext context, ILogger<MessageService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Query Operations

        /// <summary>
        /// Retrieves messages for a conversation
        /// Returns IQueryable for efficient pagination
        /// </summary>
        public async Task<IQueryable<Message>> GetMessagesAsync(Guid conversationId)
        {
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));

            _logger.LogDebug($"Loading messages for conversation {conversationId}");

            // Build query with necessary includes
            var query = _context.Messages
                .Include(m => m.ReadReceipts) // For read status
                .Include(m => m.ReplyToMessage) // For reply context
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt); // Chronological order

            // Return as IQueryable for DevExtreme to apply additional operations
            return query.AsNoTracking();
        }

        /// <summary>
        /// Gets messages within a specific date range
        /// Useful for jump-to-date functionality
        /// </summary>
        public async Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(
            Guid conversationId, 
            DateTime startDate, 
            DateTime endDate)
        {
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));

            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");

            _logger.LogDebug($"Loading messages for conversation {conversationId} between {startDate} and {endDate}");

            var messages = await _context.Messages
                .Include(m => m.ReadReceipts)
                .Where(m => m.ConversationId == conversationId)
                .Where(m => m.SentAt >= startDate && m.SentAt <= endDate)
                .OrderBy(m => m.SentAt)
                .AsNoTracking()
                .ToListAsync();

            _logger.LogDebug($"Found {messages.Count} messages in date range");
            return messages;
        }

        /// <summary>
        /// Retrieves a single message by ID
        /// </summary>
        public async Task<Message> GetMessageAsync(Guid messageId)
        {
            if (messageId == Guid.Empty)
                throw new ArgumentException("Message ID cannot be empty", nameof(messageId));

            var message = await _context.Messages
                .Include(m => m.ReadReceipts)
                .Include(m => m.ReplyToMessage)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MessageId == messageId);

            return message;
        }

        /// <summary>
        /// Searches for messages containing specific text
        /// Basic implementation - could be enhanced with full-text search
        /// </summary>
        public async Task<IEnumerable<Message>> SearchMessagesAsync(
            Guid? conversationId, 
            string searchText, 
            string userId)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<Message>();

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID required for search", nameof(userId));

            _logger.LogDebug($"Searching messages for text: '{searchText}'");

            // Build base query
            var query = _context.Messages
                .Include(m => m.Conversation)
                    .ThenInclude(c => c.Participants)
                .Where(m => !m.IsDeleted) // Don't search deleted messages
                .Where(m => m.Type == MessageType.Text); // Only search text messages

            // Filter by conversation if specified
            if (conversationId.HasValue && conversationId.Value != Guid.Empty)
            {
                query = query.Where(m => m.ConversationId == conversationId.Value);
            }
            else
            {
                // Search only in user's conversations
                query = query.Where(m => m.Conversation.Participants.Any(p => 
                    p.UserId == userId && p.LeftAt == null));
            }

            // Apply search filter (case-insensitive)
            // In production, consider using full-text search indexes
            var searchLower = searchText.ToLower();
            query = query.Where(m => m.Content.ToLower().Contains(searchLower));

            // Order by relevance (simple: most recent first)
            // Could implement scoring based on match quality
            var results = await query
                .OrderByDescending(m => m.SentAt)
                .Take(100) // Limit results
                .AsNoTracking()
                .ToListAsync();

            _logger.LogDebug($"Found {results.Count} messages matching search");
            return results;
        }

        /// <summary>
        /// Gets all unread messages for a user
        /// Used for notification badges and summaries
        /// </summary>
        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            _logger.LogDebug($"Getting unread messages for user {userId}");

            // Get user's conversations with last read times
            var conversationReadTimes = await _context.ConversationParticipants
                .Where(cp => cp.UserId == userId && cp.LeftAt == null)
                .Select(cp => new { cp.ConversationId, cp.LastReadAt })
                .ToListAsync();

            var unreadMessages = new List<Message>();

            // For each conversation, get unread messages
            // In production, this could be optimized with a single query
            foreach (var conv in conversationReadTimes)
            {
                var query = _context.Messages
                    .Where(m => m.ConversationId == conv.ConversationId)
                    .Where(m => m.SenderUserId != userId) // Not own messages
                    .Where(m => !m.IsDeleted);

                // If user has never read this conversation, all messages are unread
                if (conv.LastReadAt.HasValue)
                {
                    query = query.Where(m => m.SentAt > conv.LastReadAt.Value);
                }

                var messages = await query
                    .OrderBy(m => m.SentAt)
                    .AsNoTracking()
                    .ToListAsync();

                unreadMessages.AddRange(messages);
            }

            _logger.LogDebug($"Found {unreadMessages.Count} unread messages");
            return unreadMessages;
        }

        #endregion

        #region Command Operations

        /// <summary>
        /// Saves a new message to the database
        /// Handles alerts differently (non-persistent)
        /// </summary>
        public async Task<Message> SaveMessageAsync(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _logger.LogDebug($"Saving message {message.MessageId} to conversation {message.ConversationId}");

            // Check if this is a non-persistent alert
            if (message.Type == MessageType.Alert)
            {
                // Alerts are not saved to database
                // They're only sent through SignalR
                _logger.LogDebug("Alert message not persisted to database");
                return message;
            }

            // Validate conversation exists
            var conversationExists = await _context.Conversations
                .AnyAsync(c => c.ConversationId == message.ConversationId);

            if (!conversationExists)
                throw new InvalidOperationException($"Conversation {message.ConversationId} not found");

            // Add message to context
            _context.Messages.Add(message);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Message {message.MessageId} saved successfully");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Failed to save message {message.MessageId}");
                throw;
            }

            return message;
        }

        /// <summary>
        /// Updates an existing message
        /// Used for edits, deletes, and read receipts
        /// </summary>
        public async Task<Message> UpdateMessageAsync(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _logger.LogDebug($"Updating message {message.MessageId}");

            // Attach the message if not already tracked
            var entry = _context.Entry(message);
            if (entry.State == EntityState.Detached)
            {
                _context.Messages.Attach(message);
                entry.State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Message {message.MessageId} updated successfully");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency conflict updating message {message.MessageId}");
                throw;
            }

            return message;
        }

        /// <summary>
        /// Marks all messages in a conversation as read
        /// Bulk operation for efficiency
        /// </summary>
        public async Task<int> MarkConversationAsReadAsync(Guid conversationId, string userId)
        {
            if (conversationId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Invalid parameters");

            _logger.LogDebug($"Marking conversation {conversationId} as read for user {userId}");

            // Get all unread messages in the conversation
            var unreadMessages = await _context.Messages
                .Include(m => m.ReadReceipts)
                .Where(m => m.ConversationId == conversationId)
                .Where(m => m.SenderUserId != userId) // Not own messages
                .Where(m => !m.ReadReceipts.Any(r => r.UserId == userId)) // Not already read
                .ToListAsync();

            var readAt = DateTime.UtcNow;
            var count = 0;

            // Add read receipts for all unread messages
            foreach (var message in unreadMessages)
            {
                message.ReadReceipts.Add(new MessageReadReceipt
                {
                    MessageId = message.MessageId,
                    UserId = userId,
                    ReadAt = readAt
                });
                count++;
            }

            // Update participant's last read time
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (participant != null)
            {
                participant.LastReadAt = readAt;
                participant.UnreadCount = 0;
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Marked {count} messages as read in conversation {conversationId}");
            }

            return count;
        }

        /// <summary>
        /// Deletes old messages based on retention policy
        /// This would typically run as a scheduled job
        /// </summary>
        public async Task<int> DeleteOldMessagesAsync(Guid conversationId, DateTime beforeDate)
        {
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));

            _logger.LogWarning($"Deleting messages in conversation {conversationId} before {beforeDate}");

            // Find messages to delete
            var messagesToDelete = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Where(m => m.SentAt < beforeDate)
                .Where(m => m.Type != MessageType.System) // Don't delete system messages
                .ToListAsync();

            if (messagesToDelete.Any())
            {
                // Hard delete (remove from database)
                // In production, might want to move to archive table instead
                _context.Messages.RemoveRange(messagesToDelete);
                await _context.SaveChangesAsync();

                _logger.LogWarning($"Deleted {messagesToDelete.Count} messages");
            }

            return messagesToDelete.Count;
        }

        #endregion

        #region Analytics and Reporting

        /// <summary>
        /// Gets statistics about messages in a conversation
        /// Useful for moderation and analytics
        /// </summary>
        public async Task<MessageStatistics> GetMessageStatisticsAsync(Guid conversationId)
        {
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));

            _logger.LogDebug($"Calculating statistics for conversation {conversationId}");

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Where(m => !m.IsDeleted)
                .ToListAsync();

            if (!messages.Any())
            {
                return new MessageStatistics
                {
                    TotalMessages = 0,
                    MessagesByUser = new Dictionary<string, int>(),
                    MessagesByType = new Dictionary<string, int>(),
                    AverageMessageLength = 0
                };
            }

            var stats = new MessageStatistics
            {
                TotalMessages = messages.Count,
                FirstMessageAt = messages.Min(m => m.SentAt),
                LastMessageAt = messages.Max(m => m.SentAt),
                
                // Group by sender
                MessagesByUser = messages
                    .GroupBy(m => m.SenderUserId)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Group by type
                MessagesByType = messages
                    .GroupBy(m => m.Type.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Calculate average length (text messages only)
                AverageMessageLength = messages
                    .Where(m => m.Type == MessageType.Text)
                    .Select(m => m.Content?.Length ?? 0)
                    .DefaultIfEmpty(0)
                    .Average()
            };

            _logger.LogDebug($"Statistics calculated: {stats.TotalMessages} total messages");
            return stats;
        }

        #endregion
    }
}