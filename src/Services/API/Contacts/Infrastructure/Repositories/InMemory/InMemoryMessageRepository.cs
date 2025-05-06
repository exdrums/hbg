using API.Contacts.Domain.Models;
using API.Contacts.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace API.Contacts.Infrastructure.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of the message repository
    /// </summary>
    public class InMemoryMessageRepository : InMemoryRepositoryBase<Message>, IMessageRepository
    {
        private readonly ILogger<InMemoryMessageRepository> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public InMemoryMessageRepository(ILogger<InMemoryMessageRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the ID for a message
        /// </summary>
        protected override string GetId(Message message)
        {
            return message?.Id;
        }

        /// <summary>
        /// Gets messages for a conversation
        /// </summary>
        public async Task<IEnumerable<Message>> GetByConversationIdAsync(string conversationId, int limit = 50, DateTime? before = null)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            lock (_lock)
            {
                var query = _entities.Values
                    .Where(m => m.ConversationId == conversationId);

                if (before.HasValue)
                {
                    query = query.Where(m => m.Timestamp < before.Value);
                }

                var messages = query
                    .OrderByDescending(m => m.Timestamp)
                    .Take(limit)
                    .OrderBy(m => m.Timestamp) // Re-order chronologically for display
                    .ToList();

                return Task.FromResult(messages.AsEnumerable());
            }
        }

        /// <summary>
        /// Gets messages for a conversation after a specific timestamp
        /// </summary>
        public async Task<IEnumerable<Message>> GetByConversationIdAfterTimestampAsync(string conversationId, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            lock (_lock)
            {
                var messages = _entities.Values
                    .Where(m => m.ConversationId == conversationId)
                    .Where(m => m.Timestamp > timestamp)
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                return Task.FromResult(messages.AsEnumerable());
            }
        }

        /// <summary>
        /// Gets unread messages for a user in a conversation
        /// </summary>
        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(string conversationId, string userId, DateTime lastReadTimestamp)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                var messages = _entities.Values
                    .Where(m => m.ConversationId == conversationId)
                    .Where(m => m.AuthorId != userId) // Don't include the user's own messages
                    .Where(m => m.Timestamp > lastReadTimestamp)
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                return Task.FromResult(messages.AsEnumerable());
            }
        }

        /// <summary>
        /// Gets the count of unread messages for a user in a conversation
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string conversationId, string userId, DateTime lastReadTimestamp)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                var count = _entities.Values
                    .Count(m =>
                        m.ConversationId == conversationId &&
                        m.AuthorId != userId &&
                        m.Timestamp > lastReadTimestamp);

                return Task.FromResult(count);
            }
        }

        /// <summary>
        /// Gets messages in a thread (replies to a specific message)
        /// </summary>
        public async Task<IEnumerable<Message>> GetThreadAsync(string parentMessageId, int limit = 50)
        {
            if (string.IsNullOrEmpty(parentMessageId))
            {
                throw new ArgumentNullException(nameof(parentMessageId));
            }

            lock (_lock)
            {
                // First, get the parent message
                if (!_entities.TryGetValue(parentMessageId, out var parentMessage))
                {
                    return Task.FromResult(Enumerable.Empty<Message>());
                }

                // Then get all replies to it
                var messages = _entities.Values
                    .Where(m => m.ParentMessageId == parentMessageId)
                    .OrderBy(m => m.Timestamp)
                    .Take(limit)
                    .ToList();

                // Include the parent message at the beginning
                messages.Insert(0, parentMessage);

                return Task.FromResult(messages.AsEnumerable());
            }
        }

        /// <summary>
        /// Gets system alert messages for a conversation
        /// </summary>
        public async Task<IEnumerable<Message>> GetSystemAlertsAsync(string conversationId, int limit = 10)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            lock (_lock)
            {
                var systemAlerts = _entities.Values
                    .Where(m => m.ConversationId == conversationId)
                    .Where(m => m.IsSystemAlert)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(limit)
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                return Task.FromResult(systemAlerts.AsEnumerable());
            }
        }

        /// <summary>
        /// Gets a queryable source for messages
        /// </summary>
        public IQueryable<Message> Query()
        {
            lock (_lock)
            {
                return _entities.Values.AsQueryable();
            }
        }

        /// <summary>
        /// Searches for messages containing a text query
        /// </summary>
        public async Task<IEnumerable<Message>> SearchTextAsync(string conversationId, string searchQuery, int limit = 20)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(searchQuery))
            {
                return await Task.FromResult(Array.Empty<Message>());
            }

            lock (_lock)
            {
                var query = searchQuery.ToLowerInvariant();
                var matchingMessages = _entities.Values
                    .Where(m => m.ConversationId == conversationId)
                    .Where(m => m.Text.ToLowerInvariant().Contains(query))
                    .OrderByDescending(m => m.Timestamp)
                    .Take(limit)
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                return Task.FromResult(matchingMessages.AsEnumerable());
            }
        }
    }
}
