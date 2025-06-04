using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Models;

/// <summary>
/// Join entity representing the many-to-many relationship between 
/// Conversations and Users (participants)
/// 
/// Why a separate entity instead of automatic many-to-many?
/// 1. We can store additional metadata (JoinedAt, Role, etc.)
/// 2. Better query performance with explicit joins
/// 3. More control over the relationship
/// 4. Easier to add features like participant roles/permissions
/// 
/// This pattern is recommended for production systems where the
/// relationship itself has meaning and properties.
/// </summary>
[Table("ConversationParticipants")]
[Index(nameof(ConversationId), nameof(UserId), IsUnique = true)] // Prevent duplicates
[Index(nameof(UserId))] // For queries by user
public class ConversationParticipant
{
    /// <summary>
    /// The conversation this participation record belongs to
    /// Part of the composite primary key
    /// </summary>
    [Required]
    public Guid ConversationId { get; set; }

    /// <summary>
    /// The user who is participating in the conversation
    /// Part of the composite primary key
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UserId { get; set; }

    /// <summary>
    /// When this user joined the conversation
    /// Important for group conversations where users can be added later
    /// </summary>
    [Required]
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Optional: When the user left the conversation
    /// Null means they're still active in the conversation
    /// </summary>
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// User's role in the conversation (future feature)
    /// Could be: Member, Admin, Moderator, etc.
    /// </summary>
    [StringLength(50)]
    public string Role { get; set; } = "Member";

    /// <summary>
    /// Last time this user read messages in this conversation
    /// Used for calculating unread counts efficiently
    /// </summary>
    public DateTime? LastReadAt { get; set; }

    /// <summary>
    /// Number of unread messages for this user in this conversation
    /// Denormalized for performance - updated when messages are sent
    /// </summary>
    public int UnreadCount { get; set; } = 0;

    /// <summary>
    /// User's notification preference for this conversation
    /// Could be: All, Mentions, None
    /// </summary>
    [StringLength(50)]
    public string NotificationPreference { get; set; } = "All";

    /// <summary>
    /// Navigation property back to the conversation
    /// </summary>
    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; }

    /// <summary>
    /// Note: We don't have a navigation property to User because
    /// users are managed by the external OIDC system. We only
    /// store the user ID reference.
    /// </summary>
}
