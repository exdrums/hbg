
namespace API.Contacts.Models;

/// <summary>
/// Represents a conversation between multiple participants
/// Following DDD principles, this is our core domain entity
/// </summary>
public interface IConversation
{
    /// <summary>
    /// Unique identifier for the conversation
    /// </summary>
    Guid ConversationId { get; }
    
    /// <summary>
    /// Title of the conversation (optional for direct messages, required for group chats)
    /// </summary>
    string Title { get; set; }
    
    /// <summary>
    /// Type of conversation (Direct, Group, etc.)
    /// </summary>
    ConversationType Type { get; set; }
    
    /// <summary>
    /// List of participant user IDs in this conversation
    /// </summary>
    ICollection<string> ParticipantIds { get; set; }
    
    /// <summary>
    /// ID of the user who created the conversation
    /// </summary>
    string CreatedByUserId { get; set; }
    
    /// <summary>
    /// Timestamp when the conversation was created
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp of the last message in the conversation
    /// Used for sorting conversations by recent activity
    /// </summary>
    DateTime? LastMessageAt { get; set; }
    
    /// <summary>
    /// Indicates if the conversation is active or archived/deleted
    /// </summary>
    bool IsActive { get; set; }
    
    /// <summary>
    /// Preview of the last message for display in conversation list
    /// </summary>
    string LastMessagePreview { get; set; }
    
    /// <summary>
    /// Count of unread messages for the current user
    /// This would be calculated per user in a real implementation
    /// </summary>
    int UnreadCount { get; set; }
}
