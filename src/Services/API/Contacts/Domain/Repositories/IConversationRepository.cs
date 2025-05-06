using API.Contacts.Domain.Models;
using System;
using System.Threading.Tasks;

namespace API.Contacts.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Conversation entities
    /// </summary>
    public interface IConversationRepository : IRepository<Conversation>
    {
        /// <summary>
        /// Gets conversations for a user
        /// </summary>
        Task<IEnumerable<Conversation>> GetByUserIdAsync(string userId, bool includeArchived = false);

        /// <summary>
        /// Gets one-on-one conversation between two users, if exists
        /// </summary>
        Task<Conversation> GetOneOnOneConversationAsync(string user1Id, string user2Id);

        /// <summary>
        /// Gets AI assistant conversations for a user
        /// </summary>
        Task<IEnumerable<Conversation>> GetAiConversationsForUserAsync(string userId);

        /// <summary>
        /// Updates a conversation participant
        /// </summary>
        Task<bool> UpdateParticipantAsync(ConversationParticipant participant);

        /// <summary>
        /// Adds a participant to a conversation
        /// </summary>
        Task<bool> AddParticipantAsync(string conversationId, string userId, ParticipantRole role = ParticipantRole.Member);

        /// <summary>
        /// Removes a participant from a conversation
        /// </summary>
        Task<bool> RemoveParticipantAsync(string conversationId, string userId);

        /// <summary>
        /// Checks if a user is a participant in a conversation
        /// </summary>
        Task<bool> IsUserParticipantAsync(string conversationId, string userId);

        /// <summary>
        /// Gets conversations with unread messages for a user
        /// </summary>
        Task<IEnumerable<Conversation>> GetWithUnreadMessagesAsync(string userId);

        /// <summary>
        /// Archives a conversation for a user
        /// </summary>
        Task<bool> ArchiveAsync(string conversationId, string userId);

        /// <summary>
        /// Unarchives a conversation for a user
        /// </summary>
        Task<bool> UnarchiveAsync(string conversationId, string userId);
    }
}
