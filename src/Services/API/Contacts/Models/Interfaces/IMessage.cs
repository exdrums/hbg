namespace API.Contacts.Models;

/// <summary>
/// Represents a message within a conversation
/// Messages are immutable once created to maintain chat history integrity
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Unique identifier for the message
    /// </summary>
    Guid MessageId { get; }
    
    /// <summary>
    /// The conversation this message belongs to
    /// This creates the aggregate relationship with Conversation
    /// </summary>
    Guid ConversationId { get; }
    
    /// <summary>
    /// The user ID of the message sender
    /// </summary>
    string SenderUserId { get; }
    
    /// <summary>
    /// The actual message content
    /// Supports text, emojis, and formatted content
    /// </summary>
    string Content { get; }
    
    /// <summary>
    /// Type of message (Text, Image, File, System, etc.)
    /// </summary>
    MessageType Type { get; }
    
    /// <summary>
    /// Timestamp when the message was sent
    /// Used for ordering messages in conversation
    /// </summary>
    DateTime SentAt { get; }
    
    /// <summary>
    /// Timestamp when the message was edited (null if never edited)
    /// Messages can only be edited by the original sender
    /// </summary>
    DateTime? EditedAt { get; }
    
    /// <summary>
    /// Indicates if the message has been deleted
    /// Deleted messages show as "Message deleted" in the UI
    /// </summary>
    bool IsDeleted { get; }
    
    /// <summary>
    /// User IDs of participants who have read this message
    /// Used for read receipts functionality
    /// </summary>
    ICollection<string> ReadByUserIds { get; }
    
    /// <summary>
    /// If this is a reply to another message, contains the parent message ID
    /// Enables threaded conversation support
    /// </summary>
    Guid? ReplyToMessageId { get; }
    
    /// <summary>
    /// Optional metadata for special message types (e.g., file info, image dimensions)
    /// Stored as JSON for flexibility
    /// </summary>
    string Metadata { get; }
}
