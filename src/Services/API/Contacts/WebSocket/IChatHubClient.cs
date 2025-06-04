using API.Contacts.Models;

namespace API.Contacts.WebSocket;

/// <summary>
/// Defines the methods that the server can invoke on connected clients
/// This is the other half of the SignalR communication contract
/// 
/// Enhanced version includes alert functionality and additional client notifications
/// for comprehensive real-time chat experience
/// </summary>
public interface IChatHubClient
{
    #region Conversation Events
    
    /// <summary>
    /// Notifies clients when a new conversation is created
    /// Clients should add this to their conversation list
    /// </summary>
    /// <param name="conversation">The newly created conversation</param>
    Task ConversationCreated(Conversation conversation);
    
    /// <summary>
    /// Notifies clients when a conversation is updated
    /// Clients should update their local copy with these changes
    /// </summary>
    /// <param name="conversation">The updated conversation with new values</param>
    Task ConversationUpdated(Conversation conversation);
    
    /// <summary>
    /// Notifies clients when a conversation is archived
    /// Clients should remove or hide this conversation from active lists
    /// </summary>
    /// <param name="conversationId">ID of the archived conversation</param>
    Task ConversationArchived(Guid conversationId);
    
    /// <summary>
    /// Notifies a specific client that they left a conversation
    /// This is a personal notification, not broadcast to all
    /// </summary>
    /// <param name="conversationId">ID of the conversation they left</param>
    Task ConversationLeft(Guid conversationId);
    
    /// <summary>
    /// Notifies clients when conversation access permissions change
    /// Used for support tickets that are closed or restricted
    /// </summary>
    /// <param name="conversationId">Affected conversation ID</param>
    /// <param name="isReadOnly">Whether conversation is now read-only</param>
    /// <param name="reason">Reason for the change</param>
    Task ConversationAccessChanged(Guid conversationId, bool isReadOnly, string reason);
    
    #endregion

    #region Message Events
    
    /// <summary>
    /// Notifies clients when a new message is received
    /// This is the core real-time messaging functionality
    /// </summary>
    /// <param name="message">The new message</param>
    Task MessageReceived(Message message);
    
    /// <summary>
    /// Notifies clients when a message is edited
    /// Clients should update the existing message in their UI
    /// </summary>
    /// <param name="message">The edited message with new content</param>
    Task MessageEdited(Message message);
    
    /// <summary>
    /// Notifies clients when a message is deleted
    /// Clients should update the message to show "deleted" state
    /// </summary>
    /// <param name="messageId">ID of the deleted message</param>
    Task MessageDeleted(Guid messageId);
    
    /// <summary>
    /// Notifies clients about read receipt updates
    /// Clients can use this to show read indicators (checkmarks, etc.)
    /// </summary>
    /// <param name="messageId">The message that was read</param>
    /// <param name="userId">The user who read the message</param>
    Task MessageReadReceiptUpdated(Guid messageId, string userId);
    
    #endregion

    #region User Status Events
    
    /// <summary>
    /// Notifies clients when a user's online status changes
    /// Clients should update presence indicators in their UI
    /// </summary>
    /// <param name="userId">The user whose status changed</param>
    /// <param name="status">The new status (Online, Offline, Away, etc.)</param>
    Task UserStatusChanged(string userId, UserStatus status);
    
    /// <summary>
    /// Notifies clients when a user starts typing in a conversation
    /// Clients should show a "typing..." indicator
    /// </summary>
    /// <param name="conversationId">The conversation where typing is happening</param>
    /// <param name="userId">The user who is typing</param>
    Task UserStartedTyping(Guid conversationId, string userId);
    
    /// <summary>
    /// Notifies clients when a user stops typing
    /// Clients should remove the typing indicator
    /// </summary>
    /// <param name="conversationId">The conversation where typing stopped</param>
    /// <param name="userId">The user who stopped typing</param>
    Task UserStoppedTyping(Guid conversationId, string userId);
    
    #endregion

    #region Alert System Events
    
    /// <summary>
    /// Sends an alert message to specific clients
    /// These are non-persistent notifications for real-time feedback
    /// Examples:
    /// - "Connection lost, reconnecting..."
    /// - "You don't have permission to send messages"
    /// - "This conversation has been closed"
    /// </summary>
    /// <param name="alert">The alert message to display</param>
    Task ReceiveAlert(Message alert);
    
    /// <summary>
    /// Notifies clients of connection state changes
    /// Helps clients show connection status in the UI
    /// </summary>
    /// <param name="isConnected">True if connected, false if disconnected</param>
    /// <param name="message">Optional message explaining the state</param>
    Task ConnectionStateChanged(bool isConnected, string message = null);
    
    /// <summary>
    /// Sends a system-wide alert to clients
    /// Used for maintenance notifications, system updates, etc.
    /// </summary>
    /// <param name="alert">System alert message</param>
    Task ReceiveSystemAlert(Message alert);
    
    /// <summary>
    /// Sends permission-related alerts to clients
    /// Used when user attempts unauthorized actions
    /// </summary>
    /// <param name="alert">Permission alert message</param>
    Task ReceivePermissionAlert(Message alert);
    
    #endregion

    #region Support-Specific Events
    
    /// <summary>
    /// Notifies when a support ticket status changes
    /// Used in support conversations for status updates
    /// </summary>
    /// <param name="conversationId">Support conversation ID</param>
    /// <param name="status">New ticket status</param>
    /// <param name="assignedAgentId">Assigned support agent (if any)</param>
    Task SupportTicketStatusChanged(Guid conversationId, string status, string assignedAgentId = null);
    
    /// <summary>
    /// Notifies when support ticket priority changes
    /// </summary>
    /// <param name="conversationId">Support conversation ID</param>
    /// <param name="priority">New priority level</param>
    Task SupportTicketPriorityChanged(Guid conversationId, string priority);
    
    #endregion

    #region System Events
    
    /// <summary>
    /// Forces clients to refresh their data
    /// Used when server-side changes require a full refresh
    /// </summary>
    /// <param name="reason">Explanation of why refresh is needed</param>
    Task ForceRefresh(string reason);
    
    /// <summary>
    /// Notifies clients about system maintenance
    /// </summary>
    /// <param name="message">Maintenance message</param>
    /// <param name="startTime">When maintenance begins</param>
    /// <param name="estimatedDuration">Expected duration in minutes</param>
    Task MaintenanceNotification(string message, DateTime startTime, int estimatedDuration);
    
    #endregion

    #region DevExtreme-specific Events
    
    /// <summary>
    /// Notifies clients that they should reload a specific data source
    /// This integrates with DevExtreme's DataSource push functionality
    /// </summary>
    /// <param name="dataSourceName">Name of the data source to reload</param>
    /// <param name="parameters">Optional parameters for the reload</param>
    Task ReloadDataSource(string dataSourceName, object parameters = null);
    
    /// <summary>
    /// Pushes data changes for DevExtreme DataSource
    /// Supports the push API for real-time updates without full reload
    /// </summary>
    /// <param name="dataSourceName">Name of the data source</param>
    /// <param name="changes">Array of changes in DevExtreme format</param>
    Task PushDataSourceChanges(string dataSourceName, object[] changes);
    
    /// <summary>
    /// Notifies clients about new conversations for data source updates
    /// Used specifically for conversation list data sources
    /// </summary>
    /// <param name="conversation">The new conversation</param>
    /// <param name="conversationType">Type of conversation (Contacts, Support, Agent)</param>
    Task ConversationDataSourceInsert(Conversation conversation, string conversationType);
    
    /// <summary>
    /// Notifies clients about conversation updates for data source
    /// </summary>
    /// <param name="conversation">Updated conversation</param>
    /// <param name="conversationType">Type of conversation</param>
    Task ConversationDataSourceUpdate(Conversation conversation, string conversationType);
    
    /// <summary>
    /// Notifies clients about conversation removal from data source
    /// </summary>
    /// <param name="conversationId">Removed conversation ID</param>
    /// <param name="conversationType">Type of conversation</param>
    Task ConversationDataSourceRemove(Guid conversationId, string conversationType);
    
    #endregion
}
