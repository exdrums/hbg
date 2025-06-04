namespace API.Contacts.Models;

/// <summary>
/// Represents the online presence status of a user
/// This enum is used for real-time presence tracking in the chat system
/// 
/// Presence systems are fundamental to modern chat applications:
/// - They provide social context (who's available to chat)
/// - They help set user expectations (response times)
/// - They enable features like "last seen" timestamps
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User is currently connected and active
    /// Shows a green indicator in most UIs
    /// </summary>
    Online = 1,
    
    /// <summary>
    /// User is connected but has been idle
    /// Typically triggered after 5-10 minutes of inactivity
    /// Shows a yellow/orange indicator
    /// </summary>
    Away = 2,
    
    /// <summary>
    /// User has manually set themselves as busy
    /// Indicates they prefer not to be disturbed
    /// Shows a red indicator
    /// </summary>
    Busy = 3,
    
    /// <summary>
    /// User is not connected to the chat system
    /// Shows a gray indicator or no indicator
    /// </summary>
    Offline = 4,
    
    /// <summary>
    /// User appears offline to others
    /// They can still send/receive messages
    /// Useful for privacy-conscious users
    /// </summary>
    Invisible = 5
}
