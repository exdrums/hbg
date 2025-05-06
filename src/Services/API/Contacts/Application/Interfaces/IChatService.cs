using API.Contacts.Application.Dtos;
using API.Contacts.Domain.Models;
using System;
using System.Threading.Tasks;

namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Application service interface for chat operations
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Gets all conversations for a user
        /// </summary>
        Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(string userId);

        /// <summary>
        /// Gets a conversation by ID
        /// </summary>
        Task<ConversationDto> GetConversationByIdAsync(string conversationId, string userId);

        /// <summary>
        /// Creates a new conversation with the specified participants
        /// </summary>
        Task<ConversationDto> CreateConversationAsync(string creatorId, IEnumerable<string> participantIds, string title = null);

        /// <summary>
        /// Creates a one-on-one conversation between two users
        /// </summary>
        Task<ConversationDto> CreateOneOnOneConversationAsync(string user1Id, string user2Id);

        /// <summary>
        /// Creates a new AI assistant conversation
        /// </summary>
        Task<ConversationDto> CreateAiAssistantConversationAsync(string userId, string title = "AI Assistant");

        /// <summary>
        /// Gets messages from a conversation
        /// </summary>
        Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(string conversationId, string userId, int limit = 50, DateTime? before = null);

        /// <summary>
        /// Sends a message to a conversation
        /// </summary>
        Task<MessageDto> SendMessageAsync(string conversationId, string userId, string text, string parentMessageId = null);

        /// <summary>
        /// Sends a message to an AI assistant conversation
        /// </summary>
        Task<MessageDto> SendMessageToAiAsync(string conversationId, string userId, string text);

        /// <summary>
        /// Regenerates an AI assistant response
        /// </summary>
        Task<MessageDto> RegenerateAiResponseAsync(string conversationId, string messageId, string userId);

        /// <summary>
        /// Edits a message
        /// </summary>
        Task<MessageDto> EditMessageAsync(string messageId, string userId, string newText);

        /// <summary>
        /// Notifies that a user has started typing in a conversation
        /// </summary>
        Task UserStartedTypingAsync(string conversationId, string userId);

        /// <summary>
        /// Notifies that a user has stopped typing in a conversation
        /// </summary>
        Task UserStoppedTypingAsync(string conversationId, string userId);

        /// <summary>
        /// Gets users who are currently typing in a conversation
        /// </summary>
        Task<IEnumerable<UserDto>> GetUsersTypingAsync(string conversationId, string excludeUserId = null);

        /// <summary>
        /// Archives a conversation for a user
        /// </summary>
        Task ArchiveConversationAsync(string conversationId, string userId);

        /// <summary>
        /// Unarchives a conversation for a user
        /// </summary>
        Task UnarchiveConversationAsync(string conversationId, string userId);

        /// <summary>
        /// Adds a participant to a conversation
        /// </summary>
        Task AddParticipantAsync(string conversationId, string userId, string participantId, ParticipantRole role = ParticipantRole.Member);

        /// <summary>
        /// Removes a participant from a conversation
        /// </summary>
        Task RemoveParticipantAsync(string conversationId, string userId, string participantId);

        /// <summary>
        /// Updates a participant's role in a conversation
        /// </summary>
        Task UpdateParticipantRoleAsync(string conversationId, string userId, string participantId, ParticipantRole newRole);

        /// <summary>
        /// Marks all messages in a conversation as read by a user up to the current time
        /// </summary>
        Task MarkConversationAsReadAsync(string conversationId, string userId);

        /// <summary>
        /// Gets read receipts for a conversation (user IDs mapped to their last read timestamps)
        /// </summary>
        Task<IDictionary<string, DateTime>> GetReadReceiptsAsync(string conversationId, string userId);
    }
}
