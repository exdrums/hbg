using API.Contacts.Data;
using API.Contacts.Models;
using API.Contacts.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Services
{
    /// <summary>
    /// Implementation of conversation-related business logic
    /// 
    /// Service Layer Responsibilities:
    /// 1. Orchestrate database operations through DbContext
    /// 2. Enforce business rules that span multiple entities
    /// 3. Handle transactions when needed
    /// 4. Provide a clean API to the presentation layer (SignalR Hub)
    /// 
    /// This service is registered as Scoped in DI, matching DbContext lifetime
    /// </summary>
    public class ConversationService : IConversationService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<ConversationService> _logger;

        public ConversationService(ChatDbContext context, ILogger<ConversationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Query Operations

        /// <summary>
        /// Retrieves all conversations for a specific user
        /// Implements efficient loading with related data
        /// </summary>
        public async Task<IQueryable<Conversation>> GetUserConversationsAsync(string userId, bool includeArchived = false)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            _logger.LogDebug($"Getting conversations for user {userId}, includeArchived: {includeArchived}");

            // Build the query with proper includes for performance
            var query = _context.Conversations
                .Include(c => c.Participants) // Load participants for display
                .Where(c => c.Participants.Any(p => p.UserId == userId && p.LeftAt == null));

            // Filter out archived unless requested
            if (!includeArchived)
            {
                query = query.Where(c => c.IsActive);
            }

            // Order by most recent activity
            query = query.OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt);

            // Return IQueryable for further filtering by the caller
            // This allows DevExtreme DataSource to apply its own filtering
            return query.AsNoTracking(); // No tracking for read operations
        }

        /// <summary>
        /// Retrieves a single conversation with all related data
        /// </summary>
        public async Task<Conversation> GetConversationAsync(Guid conversationId)
        {
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));

            _logger.LogDebug($"Getting conversation {conversationId}");

            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .AsNoTracking() // Read-only operation
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation != null)
            {
                // Load unread counts for each participant
                // This would be more efficient with a custom query or view
                foreach (var participant in conversation.Participants)
                {
                    participant.UnreadCount = await GetUnreadCountAsync(conversationId, participant.UserId);
                }
            }

            return conversation;
        }

        /// <summary>
        /// Security check - verifies user has access to conversation
        /// </summary>
        public async Task<bool> UserHasAccessAsync(string userId, Guid conversationId)
        {
            if (string.IsNullOrWhiteSpace(userId) || conversationId == Guid.Empty)
                return false;

            return await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId 
                    && cp.UserId == userId 
                    && cp.LeftAt == null);
        }

        /// <summary>
        /// Finds existing direct conversation between two users
        /// Prevents duplicate direct conversations
        /// </summary>
        public async Task<Conversation> FindDirectConversationAsync(string userId1, string userId2)
        {
            if (string.IsNullOrWhiteSpace(userId1) || string.IsNullOrWhiteSpace(userId2))
                return null;

            _logger.LogDebug($"Finding direct conversation between {userId1} and {userId2}");

            // Find conversations where both users are participants
            // and it's a direct conversation (exactly 2 participants)
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .Where(c => c.Type == ConversationType.Direct && c.IsActive)
                .Where(c => c.Participants.Count == 2)
                .Where(c => c.Participants.Any(p => p.UserId == userId1 && p.LeftAt == null))
                .Where(c => c.Participants.Any(p => p.UserId == userId2 && p.LeftAt == null))
                .FirstOrDefaultAsync();

            return conversation;
        }

        #endregion

        #region Command Operations

        /// <summary>
        /// Creates a new direct conversation or returns existing one
        /// </summary>
        public async Task<Conversation> CreateDirectConversationAsync(string creatorUserId, string otherUserId)
        {
            // Check for existing conversation first
            var existing = await FindDirectConversationAsync(creatorUserId, otherUserId);
            if (existing != null)
            {
                _logger.LogInformation($"Returning existing conversation {existing.ConversationId}");
                return existing;
            }

            _logger.LogInformation($"Creating new direct conversation between {creatorUserId} and {otherUserId}");

            // Create new conversation using factory method
            var conversation = Conversation.CreateDirectConversation(creatorUserId, otherUserId);
            
            // Add to context and save
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created conversation {conversation.ConversationId}");
            return conversation;
        }

        /// <summary>
        /// Creates a new group conversation
        /// </summary>
        public async Task<Conversation> CreateGroupConversationAsync(
            string creatorUserId, 
            string title, 
            IEnumerable<string> participantIds)
        {
            _logger.LogInformation($"Creating group conversation '{title}' by {creatorUserId}");

            // Create using factory method which validates business rules
            var conversation = Conversation.CreateGroupConversation(creatorUserId, title, participantIds);
            
            // Add to context and save
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created group conversation {conversation.ConversationId}");
            return conversation;
        }

        /// <summary>
        /// Updates conversation properties
        /// Only allows updating specific fields
        /// </summary>
        public async Task<Conversation> UpdateConversationAsync(Guid conversationId, Dictionary<string, object> updates)
        {
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));

            if (updates == null || updates.Count == 0)
                throw new ArgumentException("No updates provided", nameof(updates));

            _logger.LogInformation($"Updating conversation {conversationId}");

            // Load the conversation for update (with tracking)
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null)
                throw new InvalidOperationException($"Conversation {conversationId} not found");

            // Apply allowed updates
            foreach (var update in updates)
            {
                switch (update.Key.ToLower())
                {
                    case "title":
                        if (conversation.Type == ConversationType.Group && update.Value is string title)
                        {
                            conversation.Title = title;
                            _logger.LogDebug($"Updated title to '{title}'");
                        }
                        break;

                    case "isactive":
                        if (update.Value is bool isActive)
                        {
                            conversation.IsActive = isActive;
                            _logger.LogDebug($"Updated IsActive to {isActive}");
                        }
                        break;

                    default:
                        _logger.LogWarning($"Attempted to update non-updateable field: {update.Key}");
                        break;
                }
            }

            await _context.SaveChangesAsync();
            return conversation;
        }

        /// <summary>
        /// Archives a conversation (soft delete)
        /// </summary>
        public async Task ArchiveConversationAsync(Guid conversationId)
        {
            _logger.LogInformation($"Archiving conversation {conversationId}");

            var updates = new Dictionary<string, object> { { "IsActive", false } };
            await UpdateConversationAsync(conversationId, updates);
        }

        /// <summary>
        /// Updates conversation when a new message is received
        /// </summary>
        public async Task UpdateLastMessageAsync(Guid conversationId, Message message)
        {
            if (conversationId == Guid.Empty || message == null)
                throw new ArgumentException("Invalid parameters");

            _logger.LogDebug($"Updating last message for conversation {conversationId}");

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null)
                throw new InvalidOperationException($"Conversation {conversationId} not found");

            // Update conversation's last message info
            conversation.UpdateLastMessage(message.Content);

            // Update unread counts for all participants except sender
            var participants = await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && cp.UserId != message.SenderUserId)
                .ToListAsync();

            foreach (var participant in participants)
            {
                participant.UnreadCount++;
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Participant Management

        /// <summary>
        /// Adds a participant to a group conversation
        /// </summary>
        public async Task<bool> AddParticipantAsync(Guid conversationId, string userId)
        {
            if (conversationId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
                return false;

            _logger.LogInformation($"Adding participant {userId} to conversation {conversationId}");

            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null || conversation.Type != ConversationType.Group)
                return false;

            // Use domain method to add participant
            var added = conversation.AddParticipant(userId);
            
            if (added)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully added participant {userId}");
            }

            return added;
        }

        /// <summary>
        /// Removes a participant from a group conversation
        /// </summary>
        public async Task<bool> RemoveParticipantAsync(Guid conversationId, string userId)
        {
            if (conversationId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
                return false;

            _logger.LogInformation($"Removing participant {userId} from conversation {conversationId}");

            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null || conversation.Type != ConversationType.Group)
                return false;

            try
            {
                var removed = conversation.RemoveParticipant(userId);
                
                if (removed)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully removed participant {userId}");
                }

                return removed;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to remove participant");
                return false;
            }
        }

        /// <summary>
        /// Gets all participants of a conversation
        /// </summary>
        public async Task<IEnumerable<string>> GetParticipantsAsync(Guid conversationId)
        {
            return await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && cp.LeftAt == null)
                .Select(cp => cp.UserId)
                .ToListAsync();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Calculates unread message count for a user in a conversation
        /// In a production system, this might be denormalized for performance
        /// </summary>
        private async Task<int> GetUnreadCountAsync(Guid conversationId, string userId)
        {
            var lastRead = await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && cp.UserId == userId)
                .Select(cp => cp.LastReadAt)
                .FirstOrDefaultAsync();

            if (!lastRead.HasValue)
                return 0;

            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Where(m => m.SentAt > lastRead.Value)
                .Where(m => m.SenderUserId != userId)
                .CountAsync();
        }

        #endregion
    }
}