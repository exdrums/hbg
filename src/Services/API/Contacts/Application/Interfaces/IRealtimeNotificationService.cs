using API.Contacts.Application.Dtos;
using System;
using System.Threading.Tasks;

namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Service interface for real-time notifications
    /// </summary>
    public interface IRealtimeNotificationService
    {
        /// <summary>
        /// Notifies clients about a new message in a conversation
        /// </summary>
        Task NotifyMessageReceived(string conversationId, MessageDto message);

        /// <summary>
        /// Notifies clients that a user has started typing
        /// </summary>
        Task NotifyUserStartedTyping(string conversationId, UserDto user);

        /// <summary>
        /// Notifies clients that a user has stopped typing
        /// </summary>
        Task NotifyUserStoppedTyping(string conversationId, UserDto user);

        /// <summary>
        /// Notifies clients about changes to a user's alerts
        /// </summary>
        Task NotifyAlertsChanged(string userId, IEnumerable<AlertDto> alerts);

        /// <summary>
        /// Notifies clients about updated read receipts in a conversation
        /// </summary>
        Task NotifyReadReceiptsUpdated(string conversationId, IDictionary<string, DateTime> readReceipts);

        /// <summary>
        /// Notifies clients about changes to conversation participants
        /// </summary>
        Task NotifyParticipantsChanged(string conversationId, ConversationDto updatedConversation);

        /// <summary>
        /// Subscribes a connection to a conversation's notifications
        /// </summary>
        Task SubscribeToConversation(string connectionId, string conversationId);

        /// <summary>
        /// Unsubscribes a connection from a conversation's notifications
        /// </summary>
        Task UnsubscribeFromConversation(string connectionId, string conversationId);

        /// <summary>
        /// Gets all connected user IDs for a conversation
        /// </summary>
        Task<IEnumerable<string>> GetConnectedUserIdsAsync(string conversationId);
    }
}
