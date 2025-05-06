using API.Contacts.Application.Interfaces;
using API.Contacts.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Contacts.Application.Services
{
    /// <summary>
    /// Implementation of the read receipt service for tracking message read status
    /// </summary>
    public class ReadReceiptService : IReadReceiptService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IRealtimeNotificationService _notificationService;
        private readonly ILogger<ReadReceiptService> _logger;

        // In-memory cache of read receipts (userId -> conversationId -> timestamp)
        // In a production environment, this would be stored in a persistent database
        private readonly Dictionary<string, Dictionary<string, DateTime>> _readReceipts = new();
        private readonly object _lock = new();

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ReadReceiptService(
            IMessageRepository messageRepository,
            IConversationRepository conversationRepository,
            IRealtimeNotificationService notificationService,
            ILogger<ReadReceiptService> logger)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Marks messages as read by a user up to a specific timestamp
        /// </summary>
        public async Task<int> MarkMessagesAsReadAsync(string conversationId, string userId, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Verify the conversation exists and user is a participant
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation {conversationId} not found");
                }

                var isParticipant = await _conversationRepository.IsUserParticipantAsync(conversationId, userId);
                if (!isParticipant)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not a participant in conversation {conversationId}");
                }

                // Get the previously read timestamp
                var previousReadTime = await GetLastReadTimestampAsync(userId, conversationId);

                // Count messages that were previously unread
                var unreadCount = await _messageRepository.GetUnreadCountAsync(conversationId, userId, previousReadTime);

                // Update the read receipt
                SetReadReceipt(userId, conversationId, timestamp);

                // Find the participant and update their last read timestamp
                var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant != null)
                {
                    participant.UpdateLastRead(timestamp);
                    await _conversationRepository.UpdateParticipantAsync(participant);
                }

                // Get all read receipts for the conversation
                var readReceipts = await GetReadReceiptsForConversationAsync(conversationId);

                // Notify participants about read receipt updates
                await _notificationService.NotifyReadReceiptsUpdated(conversationId, readReceipts);

                _logger.LogInformation("User {UserId} marked {Count} messages as read in conversation {ConversationId}",
                    userId, unreadCount, conversationId);

                return unreadCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read in conversation {ConversationId} for user {UserId}",
                    conversationId, userId);
                throw;
            }
        }

        /// <summary>
        /// Gets the timestamp of when a user last read a conversation
        /// </summary>
        public async Task<DateTime> GetLastReadTimestampAsync(string userId, string conversationId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            try
            {
                lock (_lock)
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last read timestamp for user {UserId} in conversation {ConversationId}",
                    userId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Gets all read receipts for a conversation
        /// </summary>
        public async Task<IDictionary<string, DateTime>> GetReadReceiptsForConversationAsync(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            try
            {
                var result = new Dictionary<string, DateTime>();

                // Get the conversation and its participants
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation {conversationId} not found");
                }

                // For each participant, get their last read timestamp
                foreach (var participant in conversation.Participants)
                {
                    var timestamp = await GetLastReadTimestampAsync(participant.UserId, conversationId);
                    result[participant.UserId] = timestamp;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting read receipts for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Gets the count of unread messages for a user in a conversation
        /// </summary>
        public async Task<int> GetUnreadMessageCountAsync(string conversationId, string userId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Get the last read timestamp
                var lastReadTimestamp = await GetLastReadTimestampAsync(userId, conversationId);

                // Get the count of unread messages
                return await _messageRepository.GetUnreadCountAsync(conversationId, userId, lastReadTimestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread message count for user {UserId} in conversation {ConversationId}",
                    userId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Sets a read receipt timestamp for a user in a conversation
        /// </summary>
        private void SetReadReceipt(string userId, string conversationId, DateTime timestamp)
        {
            lock (_lock)
            {
                // Ensure we have a dictionary for this user
                if (!_readReceipts.TryGetValue(userId, out var userReceipts))
                {
                    userReceipts = new Dictionary<string, DateTime>();
                    _readReceipts[userId] = userReceipts;
                }

                // Update the timestamp (only if newer)
                if (!userReceipts.TryGetValue(conversationId, out var existingTimestamp) || timestamp > existingTimestamp)
                {
                    userReceipts[conversationId] = timestamp;
                }
            }
        }
    }
}
