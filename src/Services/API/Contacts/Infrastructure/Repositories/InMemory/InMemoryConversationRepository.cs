using API.Contacts.Domain.Models;
using API.Contacts.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Infrastructure.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of the conversation repository
    /// </summary>
    public class InMemoryConversationRepository : InMemoryRepositoryBase<Conversation>, IConversationRepository
    {
        private readonly ILogger<InMemoryConversationRepository> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public InMemoryConversationRepository(ILogger<InMemoryConversationRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the ID for a conversation
        /// </summary>
        protected override string GetId(Conversation conversation)
        {
            return conversation?.Id;
        }

        /// <summary>
        /// Gets conversations for a user
        /// </summary>
        public async Task<IEnumerable<Conversation>> GetByUserIdAsync(string userId, bool includeArchived = false)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                var userConversations = _entities.Values
                    .Where(c => c.Participants.Any(p => p.UserId == userId))
                    .Where(c => includeArchived || !c.IsArchived)
                    .OrderByDescending(c => c.LastMessageAt)
                    .ToList();

                return Task.FromResult(userConversations.AsEnumerable());
            }
        }

        /// <summary>
        /// Gets one-on-one conversation between two users, if exists
        /// </summary>
        public async Task<Conversation> GetOneOnOneConversationAsync(string user1Id, string user2Id)
        {
            if (string.IsNullOrEmpty(user1Id))
            {
                throw new ArgumentNullException(nameof(user1Id));
            }

            if (string.IsNullOrEmpty(user2Id))
            {
                throw new ArgumentNullException(nameof(user2Id));
            }

            lock (_lock)
            {
                var oneOnOneConversation = _entities.Values
                    .Where(c => c.Type == ConversationType.OneOnOne)
                    .Where(c => c.Participants.Count() == 2)
                    .Where(c => c.Participants.Any(p => p.UserId == user1Id) && c.Participants.Any(p => p.UserId == user2Id))
                    .FirstOrDefault();

                return Task.FromResult(oneOnOneConversation);
            }
        }

        /// <summary>
        /// Gets AI assistant conversations for a user
        /// </summary>
        public async Task<IEnumerable<Conversation>> GetAiConversationsForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                var aiConversations = _entities.Values
                    .Where(c => c.Type == ConversationType.AiAssistant)
                    .Where(c => c.Participants.Any(p => p.UserId == userId))
                    .OrderByDescending(c => c.LastMessageAt)
                    .ToList();

                return Task.FromResult(aiConversations.AsEnumerable());
            }
        }

        /// <summary>
        /// Updates a conversation participant
        /// </summary>
        public async Task<bool> UpdateParticipantAsync(ConversationParticipant participant)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            lock (_lock)
            {
                if (!_entities.TryGetValue(participant.ConversationId, out var conversation))
                {
                    return Task.FromResult(false);
                }

                // The participant is already part of the conversation entity
                // and should be updated in-place, so we just need to
                // make sure the conversation entity is updated in the repository
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Adds a participant to a conversation
        /// </summary>
        public async Task<bool> AddParticipantAsync(string conversationId, string userId, ParticipantRole role = ParticipantRole.Member)
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
                if (!_entities.TryGetValue(conversationId, out var conversation))
                {
                    return Task.FromResult(false);
                }

                // Check if user is already a participant
                if (conversation.Participants.Any(p => p.UserId == userId))
                {
                    return Task.FromResult(false);
                }

                // Add the participant to the conversation
                conversation.AddParticipant(userId, role);
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Removes a participant from a conversation
        /// </summary>
        public async Task<bool> RemoveParticipantAsync(string conversationId, string userId)
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
                if (!_entities.TryGetValue(conversationId, out var conversation))
                {
                    return Task.FromResult(false);
                }

                // Check if user is a participant
                if (!conversation.Participants.Any(p => p.UserId == userId))
                {
                    return Task.FromResult(false);
                }

                // Remove the participant from the conversation
                conversation.RemoveParticipant(userId);
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Checks if a user is a participant in a conversation
        /// </summary>
        public async Task<bool> IsUserParticipantAsync(string conversationId, string userId)
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
                if (!_entities.TryGetValue(conversationId, out var conversation))
                {
                    return Task.FromResult(false);
                }

                return Task.FromResult(conversation.Participants.Any(p => p.UserId == userId));
            }
        }

        /// <summary>
        /// Gets conversations with unread messages for a user
        /// </summary>
        public async Task<IEnumerable<Conversation>> GetWithUnreadMessagesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                // In a real implementation, this would check the last read timestamp
                // against the conversation's last message timestamp
                var conversationsWithUnread = _entities.Values
                    .Where(c => c.Participants.Any(p => p.UserId == userId))
                    .Where(c => !c.IsArchived)
                    .Where(c => c.Participants.FirstOrDefault(p => p.UserId == userId)?.LastReadAt < c.LastMessageAt)
                    .OrderByDescending(c => c.LastMessageAt)
                    .ToList();

                return Task.FromResult(conversationsWithUnread.AsEnumerable());
            }
        }

        /// <summary>
        /// Archives a conversation for a user
        /// </summary>
        public async Task<bool> ArchiveAsync(string conversationId, string userId)
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
                if (!_entities.TryGetValue(conversationId, out var conversation))
                {
                    return Task.FromResult(false);
                }

                // Check if user is a participant
                if (!conversation.Participants.Any(p => p.UserId == userId))
                {
                    return Task.FromResult(false);
                }

                // In a real implementation, archiving might be user-specific
                // For simplicity, we'll archive the conversation for all users
                conversation.SetArchived(true);
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Unarchives a conversation for a user
        /// </summary>
        public async Task<bool> UnarchiveAsync(string conversationId, string userId)
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
                if (!_entities.TryGetValue(conversationId, out var conversation))
                {
                    return Task.FromResult(false);
                }

                // Check if user is a participant
                if (!conversation.Participants.Any(p => p.UserId == userId))
                {
                    return Task.FromResult(false);
                }

                // In a real implementation, unarchiving might be user-specific
                // For simplicity, we'll unarchive the conversation for all users
                conversation.SetArchived(false);
                return Task.FromResult(true);
            }
        }
    }
}
