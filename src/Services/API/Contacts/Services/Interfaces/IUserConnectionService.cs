namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Manages the mapping between users and their SignalR connections
/// 
/// Understanding the Connection Management Challenge:
/// 
/// In traditional web applications, we think of one user = one session.
/// But in real-time applications with SignalR, this becomes more complex:
/// 
/// 1. Multiple Devices: A user might be connected from multiple devices
///    (phone, laptop, tablet) simultaneously. Each device has its own
///    SignalR connection with a unique ConnectionId.
/// 
/// 2. Connection Lifecycle: SignalR connections can drop and reconnect
///    frequently (network issues, device sleep, app backgrounding).
///    We need to track these changes to maintain accurate presence.
/// 
/// 3. Presence Detection: We only want to show a user as "offline" when
///    ALL their connections are gone, not when just one device disconnects.
/// 
/// 4. Message Routing: When sending messages to a user, we need to route
///    to all their active connections to ensure delivery to all devices.
/// 
/// This service abstracts these complexities, providing a clean interface
/// for managing user-to-connection relationships. It's typically backed
/// by a fast in-memory store (like Redis) for performance.
/// </summary>
public interface IUserConnectionService
{
    #region Connection Management
    
    /// <summary>
    /// Registers a new connection for a user
    /// Called when a user connects to the SignalR hub
    /// 
    /// Implementation notes:
    /// - Should be idempotent (safe to call multiple times)
    /// - Should handle the case where connection already exists
    /// - Should update last activity timestamp
    /// - Consider storing additional metadata (device type, app version)
    /// 
    /// Example flow:
    /// 1. User opens chat on their phone → AddConnectionAsync("user123", "conn_ABC")
    /// 2. Same user opens chat on laptop → AddConnectionAsync("user123", "conn_XYZ")
    /// 3. Now GetConnectionsAsync("user123") returns ["conn_ABC", "conn_XYZ"]
    /// </summary>
    /// <param name="userId">The user who connected</param>
    /// <param name="connectionId">The SignalR connection ID</param>
    /// <returns>Task representing the async operation</returns>
    Task AddConnectionAsync(string userId, string connectionId);
    
    /// <summary>
    /// Removes a connection for a user
    /// Called when a user disconnects from the SignalR hub
    /// 
    /// Critical behavior:
    /// - Returns true if user still has other connections
    /// - Returns false if this was their last connection
    /// 
    /// This return value is crucial for presence management:
    /// - true → User is still online (has other devices connected)
    /// - false → User is now offline (no devices connected)
    /// 
    /// Example:
    /// User has 2 connections: ["conn_ABC", "conn_XYZ"]
    /// RemoveConnectionAsync("user123", "conn_ABC") → returns true
    /// RemoveConnectionAsync("user123", "conn_XYZ") → returns false (now offline)
    /// </summary>
    /// <param name="userId">The user who disconnected</param>
    /// <param name="connectionId">The SignalR connection ID to remove</param>
    /// <returns>True if user has remaining connections, false if none left</returns>
    Task<bool> RemoveConnectionAsync(string userId, string connectionId);
    
    /// <summary>
    /// Retrieves all active connections for a user
    /// Used when broadcasting messages to a specific user
    /// 
    /// Common uses:
    /// - Sending notifications to all user's devices
    /// - Adding user's connections to conversation groups
    /// - Checking if user is online (any connections = online)
    /// 
    /// Performance consideration:
    /// This is called frequently, so should be optimized for speed
    /// Consider caching or using fast in-memory storage
    /// </summary>
    /// <param name="userId">The user whose connections to retrieve</param>
    /// <returns>Collection of SignalR connection IDs</returns>
    Task<IEnumerable<string>> GetConnectionsAsync(string userId);
    
    /// <summary>
    /// Removes all connections for a user
    /// Used for forced logout or cleanup operations
    /// 
    /// Scenarios:
    /// - User explicitly logs out from all devices
    /// - Account security action (password change, suspicious activity)
    /// - Administrative action (ban, suspension)
    /// - Cleanup during testing or maintenance
    /// </summary>
    /// <param name="userId">The user whose connections to remove</param>
    /// <returns>Number of connections that were removed</returns>
    Task<int> RemoveAllConnectionsAsync(string userId);
    
    #endregion

    #region User Lookup
    
    /// <summary>
    /// Finds which user owns a specific connection
    /// Reverse lookup from connection ID to user ID
    /// 
    /// This is needed because SignalR gives us ConnectionId in events,
    /// but we often need to know which user that connection belongs to.
    /// 
    /// Use cases:
    /// - Handling disconnection events (who disconnected?)
    /// - Security checks (does this connection belong to claimed user?)
    /// - Debugging and monitoring
    /// 
    /// Should return null if connection not found
    /// </summary>
    /// <param name="connectionId">The SignalR connection ID</param>
    /// <returns>The user ID or null if not found</returns>
    Task<string> GetUserIdByConnectionAsync(string connectionId);
    
    #endregion

    #region Presence Tracking
    
    /// <summary>
    /// Checks if a user has any active connections
    /// Simple way to determine online/offline status
    /// 
    /// This is more efficient than calling GetConnectionsAsync
    /// and checking if the collection is empty
    /// </summary>
    /// <param name="userId">The user to check</param>
    /// <returns>True if user has at least one connection</returns>
    Task<bool> IsUserOnlineAsync(string userId);
    
    /// <summary>
    /// Gets all users who are currently online
    /// Used for presence indicators and user lists
    /// 
    /// Performance warning:
    /// This could return a large set in busy systems
    /// Consider pagination or limiting scope (e.g., friends only)
    /// 
    /// Advanced features to consider:
    /// - Filter by user's contact list
    /// - Include last activity timestamp
    /// - Support for "invisible" mode
    /// </summary>
    /// <returns>Collection of online user IDs</returns>
    Task<IEnumerable<string>> GetOnlineUsersAsync();
    
    /// <summary>
    /// Updates the last activity timestamp for a user
    /// Used to track user activity for "away" status
    /// 
    /// Call this when user performs actions:
    /// - Sends a message
    /// - Opens a conversation  
    /// - Interacts with the UI
    /// 
    /// The timestamp can be used to automatically set
    /// users to "away" status after inactivity
    /// </summary>
    /// <param name="userId">The active user</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdateUserActivityAsync(string userId);
    
    #endregion

    #region Connection Metadata
    
    /// <summary>
    /// Stores additional information about a connection
    /// Enables rich presence features
    /// 
    /// Metadata examples:
    /// - Device type (mobile, desktop, web)
    /// - App version
    /// - Location (if user permits)
    /// - Connection quality metrics
    /// 
    /// This data enhances user experience:
    /// - Show device icons in active sessions
    /// - Route notifications appropriately
    /// - Debug connection issues
    /// </summary>
    /// <param name="connectionId">The connection to annotate</param>
    /// <param name="metadata">Key-value pairs of metadata</param>
    /// <returns>Task representing the async operation</returns>
    Task SetConnectionMetadataAsync(string connectionId, Dictionary<string, string> metadata);
    
    /// <summary>
    /// Retrieves metadata for a connection
    /// </summary>
    /// <param name="connectionId">The connection to query</param>
    /// <returns>Metadata dictionary or empty if none</returns>
    Task<Dictionary<string, string>> GetConnectionMetadataAsync(string connectionId);
    
    #endregion
}
