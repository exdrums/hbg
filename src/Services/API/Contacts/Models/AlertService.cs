using API.Contacts.Models;
using API.Contacts.Services.Interfaces;
using API.Contacts.WebSocket;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace API.Contacts.Services;

/// <summary>
/// Implementation of alert service for sending real-time notifications
/// 
/// This service integrates with SignalR to send non-persistent alerts
/// to users for various system events and notifications.
/// 
/// Design considerations:
/// 1. Alerts are not persisted to database (unlike regular messages)
/// 2. Delivery is best-effort through SignalR
/// 3. Alerts have different types for UI styling
/// 4. Service works with user connection tracking for targeting
/// </summary>
public class AlertService : IAlertService
{
    private readonly IHubContext<ChatHub, IChatHubClient> _hubContext;
    private readonly IUserConnectionService _connectionService;
    private readonly IConversationService _conversationService;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IHubContext<ChatHub, IChatHubClient> hubContext,
        IUserConnectionService connectionService,
        IConversationService conversationService,
        ILogger<AlertService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Alert Creation

    /// <summary>
    /// Creates an informational alert for a specific user
    /// </summary>
    public async Task<Message> CreateInfoAlertAsync(string userId, string message, Guid? conversationId = null)
    {
        return await CreateAndSendAlertAsync(userId, message, AlertType.Info, conversationId);
    }

    /// <summary>
    /// Creates a warning alert for a specific user
    /// </summary>
    public async Task<Message> CreateWarningAlertAsync(string userId, string message, Guid? conversationId = null)
    {
        return await CreateAndSendAlertAsync(userId, message, AlertType.Warning, conversationId);
    }

    /// <summary>
    /// Creates an error alert for a specific user
    /// </summary>
    public async Task<Message> CreateErrorAlertAsync(string userId, string message, Guid? conversationId = null)
    {
        return await CreateAndSendAlertAsync(userId, message, AlertType.Error, conversationId);
    }

    /// <summary>
    /// Creates a success alert for a specific user
    /// </summary>
    public async Task<Message> CreateSuccessAlertAsync(string userId, string message, Guid? conversationId = null)
    {
        return await CreateAndSendAlertAsync(userId, message, AlertType.Success, conversationId);
    }

    #endregion

    #region Broadcast Alerts

    /// <summary>
    /// Sends an alert to all participants in a conversation
    /// </summary>
    public async Task BroadcastConversationAlertAsync(
        Guid conversationId, 
        string message, 
        AlertType alertType = AlertType.Info,
        string excludeUserId = null)
    {
        try
        {
            _logger.LogInformation($"Broadcasting {alertType} alert to conversation {conversationId}: {message}");

            var participants = await _conversationService.GetParticipantsAsync(conversationId);
            var targetParticipants = excludeUserId != null 
                ? participants.Where(p => p != excludeUserId) 
                : participants;

            var alert = CreateAlertMessage(conversationId, message, alertType);

            // Send to all participants through SignalR group
            await _hubContext.Clients.Group($"conversation_{conversationId}").ReceiveAlert(alert);

            _logger.LogInformation($"Alert broadcast to {targetParticipants.Count()} participants");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to broadcast alert to conversation {conversationId}");
            throw;
        }
    }

    /// <summary>
    /// Sends an alert to all online users
    /// </summary>
    public async Task BroadcastSystemAlertAsync(string message, AlertType alertType = AlertType.Info)
    {
        try
        {
            _logger.LogWarning($"Broadcasting system {alertType} alert: {message}");

            var alert = CreateAlertMessage(null, message, alertType, isSystemWide: true);

            // Send to all connected clients
            await _hubContext.Clients.All.ReceiveAlert(alert);

            var onlineUsers = await _connectionService.GetOnlineUsersAsync();
            _logger.LogWarning($"System alert broadcast to {onlineUsers.Count()} online users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast system alert");
            throw;
        }
    }

    /// <summary>
    /// Sends an alert to users with specific roles/permissions
    /// </summary>
    public async Task BroadcastRoleAlertAsync(string requiredClaim, string message, AlertType alertType = AlertType.Info)
    {
        try
        {
            _logger.LogInformation($"Broadcasting {alertType} alert to users with claim '{requiredClaim}': {message}");

            var alert = CreateAlertMessage(null, message, alertType);

            // Send to users with specific claim through group membership
            // Users are added to role-based groups during connection
            await _hubContext.Clients.Group($"role_{requiredClaim}").ReceiveAlert(alert);

            _logger.LogInformation($"Role-based alert broadcast completed for '{requiredClaim}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to broadcast role alert for claim '{requiredClaim}'");
            throw;
        }
    }

    #endregion

    #region Connection Status Alerts

    /// <summary>
    /// Sends connection established alert to a user
    /// </summary>
    public async Task SendConnectionEstablishedAlertAsync(string userId)
    {
        await SendUserSpecificAlertAsync(
            userId, 
            "Connected to chat service", 
            AlertType.Success,
            autoHide: true);
    }

    /// <summary>
    /// Sends connection lost alert to a user
    /// </summary>
    public async Task SendConnectionLostAlertAsync(string userId)
    {
        await SendUserSpecificAlertAsync(
            userId, 
            "Connection lost. Attempting to reconnect...", 
            AlertType.Warning,
            persistent: true);
    }

    /// <summary>
    /// Sends reconnection alert to a user
    /// </summary>
    public async Task SendReconnectedAlertAsync(string userId)
    {
        await SendUserSpecificAlertAsync(
            userId, 
            "Reconnected successfully", 
            AlertType.Success,
            autoHide: true);
    }

    #endregion

    #region Permission Alerts

    /// <summary>
    /// Sends permission denied alert when user tries unauthorized action
    /// </summary>
    public async Task SendPermissionDeniedAlertAsync(string userId, string action, Guid? conversationId = null)
    {
        var message = $"Permission denied: {action}";
        
        await SendUserSpecificAlertAsync(userId, message, AlertType.Error, conversationId);
        
        _logger.LogWarning($"Permission denied for user {userId}: {action}");
    }

    /// <summary>
    /// Sends read-only mode alert for conversations
    /// </summary>
    public async Task SendReadOnlyModeAlertAsync(string userId, Guid conversationId, string reason)
    {
        var message = $"This conversation is read-only: {reason}";
        
        await SendUserSpecificAlertAsync(userId, message, AlertType.Warning, conversationId, persistent: true);
    }

    #endregion

    #region Support-Specific Alerts

    /// <summary>
    /// Sends alert when support ticket is closed
    /// </summary>
    public async Task SendSupportTicketClosedAlertAsync(Guid conversationId, string closedByUserId)
    {
        var message = "This support ticket has been closed. You can no longer send messages.";
        
        await BroadcastConversationAlertAsync(
            conversationId, 
            message, 
            AlertType.Info, 
            excludeUserId: closedByUserId);
    }

    /// <summary>
    /// Sends alert when support agent is assigned to a ticket
    /// </summary>
    public async Task SendSupportAgentAssignedAlertAsync(Guid conversationId, string agentUserId)
    {
        var message = "A support agent has been assigned to your ticket.";
        
        await BroadcastConversationAlertAsync(
            conversationId, 
            message, 
            AlertType.Info,
            excludeUserId: agentUserId);
    }

    /// <summary>
    /// Sends alert when support ticket priority changes
    /// </summary>
    public async Task SendSupportPriorityChangedAlertAsync(Guid conversationId, string newPriority)
    {
        var message = $"Ticket priority has been changed to: {newPriority}";
        
        await BroadcastConversationAlertAsync(conversationId, message, AlertType.Info);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Creates and sends an alert to a specific user
    /// </summary>
    private async Task<Message> CreateAndSendAlertAsync(
        string userId, 
        string message, 
        AlertType alertType, 
        Guid? conversationId = null)
    {
        var alert = CreateAlertMessage(conversationId, message, alertType);
        
        await SendUserSpecificAlertAsync(userId, message, alertType, conversationId);
        
        return alert;
    }

    /// <summary>
    /// Sends an alert to a specific user through their active connections
    /// </summary>
    private async Task SendUserSpecificAlertAsync(
        string userId, 
        string message, 
        AlertType alertType, 
        Guid? conversationId = null,
        bool persistent = false,
        bool autoHide = false)
    {
        try
        {
            var alert = CreateAlertMessage(conversationId, message, alertType, persistent, autoHide);
            
            // Get user's active connections
            var connections = await _connectionService.GetConnectionsAsync(userId);
            
            if (connections.Any())
            {
                // Send to all user's connections
                await _hubContext.Clients.Clients(connections).ReceiveAlert(alert);
                
                _logger.LogDebug($"Alert sent to user {userId} on {connections.Count()} connections: {message}");
            }
            else
            {
                _logger.LogWarning($"User {userId} has no active connections for alert: {message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send alert to user {userId}: {message}");
            throw;
        }
    }

    /// <summary>
    /// Creates an alert message with appropriate metadata
    /// </summary>
    private Message CreateAlertMessage(
        Guid? conversationId, 
        string content, 
        AlertType alertType,
        bool isSystemWide = false,
        bool persistent = false,
        bool autoHide = false)
    {
        var metadata = new
        {
            alertType = alertType.ToString().ToLower(),
            isPersistent = persistent,
            autoHide = autoHide,
            isSystemWide = isSystemWide,
            timestamp = DateTime.UtcNow
        };

        var alert = Message.CreateAlertMessage(
            conversationId ?? Guid.Empty, 
            content,
            alertType.ToString().ToLower());

        // Override metadata with our enhanced version
        alert.Metadata = JsonSerializer.Serialize(metadata);

        return alert;
    }

    #endregion
}
