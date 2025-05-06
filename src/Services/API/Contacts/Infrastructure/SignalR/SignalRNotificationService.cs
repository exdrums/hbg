using API.Contacts.Application.Dtos;
using API.Contacts.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Infrastructure.SignalR
{
    /// <summary>
    /// Implementation of the real-time notification service using SignalR
    /// </summary>
    public class SignalRNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger<SignalRNotificationService> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public SignalRNotificationService(
            IHubContext<ChatHub> hubContext,
            IConnectionManager connectionManager,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Notifies clients about a new message in a conversation
        /// </summary>
        public async Task NotifyMessageReceived(string conversationId, MessageDto message)
        {
            try
            {
                // Get all connection IDs for this conversation group
                var connections = _connectionManager.GetConnectionsForConversation(conversationId).ToList();

                if (connections.Any())
                {
                    // Send the message to all clients connected to this conversation
                    await _hubContext.Clients
                        .Clients(connections)
                        .SendAsync("ReceiveMessage", conversationId, message);

                    _logger.LogDebug("Notified {ConnectionCount} connections about message {MessageId} in conversation {ConversationId}",
                        connections.Count, message.Id, conversationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about message {MessageId} in conversation {ConversationId}",
                    message.Id, conversationId);
            }
        }

        /// <summary>
        /// Notifies clients that a user has started typing
        /// </summary>
        public async Task NotifyUserStartedTyping(string conversationId, UserDto user)
        {
            try
            {
                var connections = _connectionManager.GetConnectionsForConversation(conversationId).ToList();

                if (connections.Any())
                {
                    await _hubContext.Clients
                        .Clients(connections)
                        .SendAsync("UserStartedTyping", conversationId, user);

                    _logger.LogDebug("Notified {ConnectionCount} connections that user {UserId} started typing in conversation {ConversationId}",
                        connections.Count, user.Id, conversationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying that user {UserId} started typing in conversation {ConversationId}",
                    user.Id, conversationId);
            }
        }

        /// <summary>
        /// Notifies clients that a user has stopped typing
        /// </summary>
        public async Task NotifyUserStoppedTyping(string conversationId, UserDto user)
        {
            try
            {
                var connections = _connectionManager.GetConnectionsForConversation(conversationId).ToList();

                if (connections.Any())
                {
                    await _hubContext.Clients
                        .Clients(connections)
                        .SendAsync("UserStoppedTyping", conversationId, user);

                    _logger.LogDebug("Notified {ConnectionCount} connections that user {UserId} stopped typing in conversation {ConversationId}",
                        connections.Count, user.Id, conversationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying that user {UserId} stopped typing in conversation {ConversationId}",
                    user.Id, conversationId);
            }
        }

        /// <summary>
        /// Notifies clients about changes to a user's alerts
        /// </summary>
        public async Task NotifyAlertsChanged(string userId, IEnumerable<AlertDto> alerts)
        {
            try
            {
                var connections = _connectionManager.GetConnectionsForUser(userId).ToList();

                if (connections.Any())
                {
                    await _hubContext.Clients
                        .Clients(connections)
                        .SendAsync("AlertsChanged", alerts);

                    _logger.LogDebug("Notified user {UserId} about {AlertCount} alerts on {ConnectionCount} connections",
                        userId, alerts.Count(), connections.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying user {UserId} about alerts", userId);
            }
        }

        /// <summary>
        /// Notifies clients about updated read receipts in a conversation
        /// </summary>
        public async Task NotifyReadReceiptsUpdated(string conversationId, IDictionary<string, DateTime> readReceipts)
        {
            try
            {
                var connections = _connectionManager.GetConnectionsForConversation(conversationId).ToList();

                if (connections.Any())
                {
                    await _hubContext.Clients
                        .Clients(connections)
                        .SendAsync("ReadReceiptsUpdated", conversationId, readReceipts);

                    _logger.LogDebug("Notified {ConnectionCount} connections about read receipts in conversation {ConversationId}",
                        connections.Count, conversationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about read receipts in conversation {ConversationId}", conversationId);
            }
        }

        /// <summary>
        /// Notifies clients about changes to conversation participants
        /// </summary>
        public async Task NotifyParticipantsChanged(string conversationId, ConversationDto updatedConversation)
        {
            try
            {
                var connections = _connectionManager.GetConnectionsForConversation(conversationId).ToList();

                if (connections.Any())
                {
                    await _hubContext.Clients
                        .Clients(connections)
                        .SendAsync("ParticipantsChanged", conversationId, updatedConversation.Participants);

                    _logger.LogDebug("Notified {ConnectionCount} connections about participant changes in conversation {ConversationId}",
                        connections.Count, conversationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about participant changes in conversation {ConversationId}", conversationId);
            }
        }

        /// <summary>
        /// Subscribes a connection to a conversation's notifications
        /// </summary>
        public async Task SubscribeToConversation(string connectionId, string conversationId)
        {
            try
            {
                _connectionManager.AddToConversation(connectionId, conversationId);
                _logger.LogDebug("Connection {ConnectionId} subscribed to conversation {ConversationId}", connectionId, conversationId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing connection {ConnectionId} to conversation {ConversationId}",
                    connectionId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Unsubscribes a connection from a conversation's notifications
        /// </summary>
        public async Task UnsubscribeFromConversation(string connectionId, string conversationId)
        {
            try
            {
                _connectionManager.RemoveFromConversation(connectionId, conversationId);
                _logger.LogDebug("Connection {ConnectionId} unsubscribed from conversation {ConversationId}", connectionId, conversationId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing connection {ConnectionId} from conversation {ConversationId}",
                    connectionId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Gets all connected user IDs for a conversation
        /// </summary>
        public async Task<IEnumerable<string>> GetConnectedUserIdsAsync(string conversationId)
        {
            try
            {
                var connections = _connectionManager.GetConnectionsForConversation(conversationId);
                var userIds = new HashSet<string>();

                foreach (var connectionId in connections)
                {
                    var userId = _connectionManager.GetUserIdForConnection(connectionId);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        userIds.Add(userId);
                    }
                }

                return userIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connected user IDs for conversation {ConversationId}", conversationId);
                throw;
            }
        }
    }
}
