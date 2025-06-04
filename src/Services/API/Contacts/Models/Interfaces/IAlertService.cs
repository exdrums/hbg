using API.Contacts.Models;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Service for managing system alerts sent to users in real-time
/// 
/// Alerts are non-persistent messages used for system notifications:
/// - Connection status changes
/// - Permission errors
/// - System maintenance notifications
/// - Support chat status updates
/// 
/// Unlike regular messages, alerts are not stored in the database
/// and are sent directly through SignalR for immediate delivery
/// </summary>
public interface IAlertService
{
    #region Alert Creation
    
    /// <summary>
    /// Creates an informational alert for a specific user
    /// Used for general notifications and status updates
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="message">Alert message content</param>
    /// <param name="conversationId">Optional conversation context</param>
    /// <returns>The created alert message</returns>
    Task<Message> CreateInfoAlertAsync(string userId, string message, Guid? conversationId = null);
    
    /// <summary>
    /// Creates a warning alert for a specific user
    /// Used for non-critical issues that require user attention
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="message">Warning message content</param>
    /// <param name="conversationId">Optional conversation context</param>
    /// <returns>The created alert message</returns>
    Task<Message> CreateWarningAlertAsync(string userId, string message, Guid? conversationId = null);
    
    /// <summary>
    /// Creates an error alert for a specific user
    /// Used for critical issues and permission errors
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="message">Error message content</param>
    /// <param name="conversationId">Optional conversation context</param>
    /// <returns>The created alert message</returns>
    Task<Message> CreateErrorAlertAsync(string userId, string message, Guid? conversationId = null);
    
    /// <summary>
    /// Creates a success alert for a specific user
    /// Used for confirmation of successful operations
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="message">Success message content</param>
    /// <param name="conversationId">Optional conversation context</param>
    /// <returns>The created alert message</returns>
    Task<Message> CreateSuccessAlertAsync(string userId, string message, Guid? conversationId = null);
    
    #endregion

    #region Broadcast Alerts
    
    /// <summary>
    /// Sends an alert to all participants in a conversation
    /// Used for conversation-specific notifications
    /// </summary>
    /// <param name="conversationId">Target conversation</param>
    /// <param name="message">Alert message content</param>
    /// <param name="alertType">Type of alert (info, warning, error, success)</param>
    /// <param name="excludeUserId">Optional user ID to exclude from broadcast</param>
    /// <returns>Task representing the async operation</returns>
    Task BroadcastConversationAlertAsync(
        Guid conversationId, 
        string message, 
        AlertType alertType = AlertType.Info,
        string excludeUserId = null);
    
    /// <summary>
    /// Sends an alert to all online users
    /// Used for system-wide notifications
    /// </summary>
    /// <param name="message">Alert message content</param>
    /// <param name="alertType">Type of alert</param>
    /// <returns>Task representing the async operation</returns>
    Task BroadcastSystemAlertAsync(string message, AlertType alertType = AlertType.Info);
    
    /// <summary>
    /// Sends an alert to users with specific roles/permissions
    /// Used for role-based notifications (e.g., support staff)
    /// </summary>
    /// <param name="requiredClaim">Required claim/permission</param>
    /// <param name="message">Alert message content</param>
    /// <param name="alertType">Type of alert</param>
    /// <returns>Task representing the async operation</returns>
    Task BroadcastRoleAlertAsync(string requiredClaim, string message, AlertType alertType = AlertType.Info);
    
    #endregion

    #region Connection Status Alerts
    
    /// <summary>
    /// Sends connection established alert to a user
    /// Called when user successfully connects to the chat
    /// </summary>
    /// <param name="userId">The connected user</param>
    /// <returns>Task representing the async operation</returns>
    Task SendConnectionEstablishedAlertAsync(string userId);
    
    /// <summary>
    /// Sends connection lost alert to a user
    /// Called when connection issues are detected
    /// </summary>
    /// <param name="userId">The affected user</param>
    /// <returns>Task representing the async operation</returns>
    Task SendConnectionLostAlertAsync(string userId);
    
    /// <summary>
    /// Sends reconnection alert to a user
    /// Called when user successfully reconnects after connection loss
    /// </summary>
    /// <param name="userId">The reconnected user</param>
    /// <returns>Task representing the async operation</returns>
    Task SendReconnectedAlertAsync(string userId);
    
    #endregion

    #region Permission Alerts
    
    /// <summary>
    /// Sends permission denied alert when user tries unauthorized action
    /// Common scenarios: sending messages to closed conversations, accessing restricted features
    /// </summary>
    /// <param name="userId">The user who was denied</param>
    /// <param name="action">The action that was attempted</param>
    /// <param name="conversationId">Optional conversation context</param>
    /// <returns>Task representing the async operation</returns>
    Task SendPermissionDeniedAlertAsync(string userId, string action, Guid? conversationId = null);
    
    /// <summary>
    /// Sends read-only mode alert for conversations
    /// Used when support chats are closed or conversation is archived
    /// </summary>
    /// <param name="userId">The user to notify</param>
    /// <param name="conversationId">The affected conversation</param>
    /// <param name="reason">Reason for read-only mode</param>
    /// <returns>Task representing the async operation</returns>
    Task SendReadOnlyModeAlertAsync(string userId, Guid conversationId, string reason);
    
    #endregion

    #region Support-Specific Alerts
    
    /// <summary>
    /// Sends alert when support ticket is closed
    /// Notifies both user and support agent
    /// </summary>
    /// <param name="conversationId">Support conversation ID</param>
    /// <param name="closedByUserId">User who closed the ticket</param>
    /// <returns>Task representing the async operation</returns>
    Task SendSupportTicketClosedAlertAsync(Guid conversationId, string closedByUserId);
    
    /// <summary>
    /// Sends alert when support agent is assigned to a ticket
    /// </summary>
    /// <param name="conversationId">Support conversation ID</param>
    /// <param name="agentUserId">Assigned support agent</param>
    /// <returns>Task representing the async operation</returns>
    Task SendSupportAgentAssignedAlertAsync(Guid conversationId, string agentUserId);
    
    /// <summary>
    /// Sends alert when support ticket priority changes
    /// </summary>
    /// <param name="conversationId">Support conversation ID</param>
    /// <param name="newPriority">New priority level</param>
    /// <returns>Task representing the async operation</returns>
    Task SendSupportPriorityChangedAlertAsync(Guid conversationId, string newPriority);
    
    #endregion
}

/// <summary>
/// Enumeration for different alert types
/// Determines the visual styling and urgency of alerts
/// </summary>
public enum AlertType
{
    /// <summary>
    /// Informational alert - neutral styling
    /// Used for general notifications and status updates
    /// </summary>
    Info = 1,
    
    /// <summary>
    /// Success alert - positive styling (green)
    /// Used for confirmation messages
    /// </summary>
    Success = 2,
    
    /// <summary>
    /// Warning alert - caution styling (yellow/orange)
    /// Used for non-critical issues
    /// </summary>
    Warning = 3,
    
    /// <summary>
    /// Error alert - critical styling (red)
    /// Used for errors and critical issues
    /// </summary>
    Error = 4
}
