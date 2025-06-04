namespace API.Contacts.Models;

/// <summary>
/// Defines the different types of messages that can be sent in a conversation
/// Each type may have different rendering and behavior in the UI
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Standard text message
    /// The most common type, supports plain text and basic formatting
    /// </summary>
    Text = 1,
    
    /// <summary>
    /// Image message
    /// Contains a URL or base64 encoded image data in metadata
    /// </summary>
    Image = 2,
    
    /// <summary>
    /// File attachment
    /// Contains file information (name, size, URL) in metadata
    /// </summary>
    File = 3,
    
    /// <summary>
    /// System-generated message
    /// Used for notifications like "User joined conversation" or "Title changed"
    /// Cannot be edited or deleted by users
    /// </summary>
    System = 4,
    
    /// <summary>
    /// Audio message or voice note
    /// Contains audio file information and duration in metadata
    /// </summary>
    Audio = 5,
    
    /// <summary>
    /// Video message
    /// Contains video file information and thumbnail in metadata
    /// </summary>
    Video = 6,
    
    /// <summary>
    /// Location sharing message
    /// Contains GPS coordinates and optional location name in metadata
    /// </summary>
    Location = 7,
    
    /// <summary>
    /// Alert message from the system
    /// Non-persistent, used for temporary notifications
    /// These align with the DxChat Alert feature
    /// </summary>
    Alert = 8
}
