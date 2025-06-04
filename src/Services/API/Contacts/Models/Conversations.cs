using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Models;

/// <summary>
/// Concrete implementation of the Conversation entity with EF Core mapping
/// This represents the aggregate root for our conversation bounded context
/// 
/// Entity Framework Core Design Decisions:
/// 1. We use Data Annotations for configuration (clearer than Fluent API)
/// 2. Collections are handled through separate join tables
/// 3. Computed properties are marked with [NotMapped]
/// 4. Indexes are defined for common query patterns
/// </summary>
[Table("Conversations")]
[Index(nameof(IsActive), nameof(LastMessageAt))] // For loading active conversations
[Index(nameof(Type))] // For filtering by conversation type
public class Conversation : IConversation
{
    /// <summary>
    /// Private constructor ensures object creation through factory method
    /// This enforces domain invariants during creation
    /// </summary>
    public Conversation()
    {
        // EF Core requires a parameterless constructor
        // Initialize collections to prevent null reference issues
        Participants = new HashSet<ConversationParticipant>();
        Messages = new HashSet<Message>();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Primary key for the conversation
    /// Using GUID for distributed system compatibility
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] // We generate GUIDs in code
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Title of the conversation
    /// Nullable for direct messages (title derived from participants)
    /// </summary>
    [StringLength(200)]
    public string Title { get; set; }

    /// <summary>
    /// Type of conversation determines behavior and UI
    /// Stored as integer in database
    /// </summary>
    [Required]
    public ConversationType Type { get; set; }

    /// <summary>
    /// User who created the conversation
    /// Foreign key to the user system (handled externally via OIDC)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// When the conversation was created
    /// Used for auditing and sorting
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the last message was sent
    /// Nullable because new conversations have no messages
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// Soft delete flag - archived conversations remain in DB
    /// This preserves message history while hiding from active lists
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// Preview of the last message for list display
    /// Denormalized for performance (avoid joining messages table)
    /// </summary>
    [StringLength(500)]
    public string? LastMessagePreview { get; set; }

    /// <summary>
    /// Navigation property for participants
    /// EF Core will create a join table ConversationParticipants
    /// </summary>
    public virtual ICollection<ConversationParticipant> Participants { get; set; }

    /// <summary>
    /// Navigation property for messages in this conversation
    /// One-to-many relationship with cascade delete
    /// </summary>
    public virtual ICollection<Message> Messages { get; set; }

    #region IConversation Implementation

    /// <summary>
    /// Computed property - participant IDs extracted from join table
    /// Not mapped to database
    /// </summary>
    [NotMapped]
    public ICollection<string> ParticipantIds
    {
        get => Participants?.Select(p => p.UserId).ToList() ?? new List<string>();
        set
        {
            // This setter is for compatibility with the interface
            // In practice, use the Participants navigation property
            throw new NotSupportedException("Use Participants navigation property instead");
        }
    }

    /// <summary>
    /// Unread count is user-specific and not stored here
    /// This would be calculated from a separate UserConversationState table
    /// </summary>
    [NotMapped]
    public int UnreadCount { get; set; }

    #endregion

    #region Factory Methods (Same as before)

    /// <summary>
    /// Factory method for creating a new direct conversation between two users
    /// Ensures all business rules are satisfied during creation
    /// </summary>
    public static Conversation CreateDirectConversation(string creatorUserId, string otherUserId)
    {
        if (string.IsNullOrWhiteSpace(creatorUserId))
            throw new ArgumentException("Creator user ID cannot be empty", nameof(creatorUserId));
        
        if (string.IsNullOrWhiteSpace(otherUserId))
            throw new ArgumentException("Other user ID cannot be empty", nameof(otherUserId));
        
        if (creatorUserId == otherUserId)
            throw new ArgumentException("Cannot create a conversation with yourself");

        var conversation = new Conversation
        {
            ConversationId = Guid.NewGuid(),
            Type = ConversationType.Direct,
            CreatedByUserId = creatorUserId,
            Title = null // Direct conversations don't need titles
        };

        // Add both participants through the proper navigation property
        conversation.Participants.Add(new ConversationParticipant 
        { 
            ConversationId = conversation.ConversationId, 
            UserId = creatorUserId,
            JoinedAt = DateTime.UtcNow
        });
        conversation.Participants.Add(new ConversationParticipant 
        { 
            ConversationId = conversation.ConversationId, 
            UserId = otherUserId,
            JoinedAt = DateTime.UtcNow
        });

        return conversation;
    }

    /// <summary>
    /// Factory method for creating a new group conversation
    /// Validates that group conversations have proper titles and multiple participants
    /// </summary>
    public static Conversation CreateGroupConversation(string creatorUserId, string title, IEnumerable<string> participantIds)
    {
        if (string.IsNullOrWhiteSpace(creatorUserId))
            throw new ArgumentException("Creator user ID cannot be empty", nameof(creatorUserId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Group conversations must have a title", nameof(title));
        
        if (participantIds == null)
            throw new ArgumentNullException(nameof(participantIds));

        var conversation = new Conversation
        {
            ConversationId = Guid.NewGuid(),
            Type = ConversationType.Group,
            CreatedByUserId = creatorUserId,
            Title = title
        };

        // Ensure creator is included in participants
        var participantSet = new HashSet<string>(participantIds);
        participantSet.Add(creatorUserId);

        if (participantSet.Count < 2)
            throw new ArgumentException("Group conversations must have at least 2 participants");

        foreach (var participantId in participantSet)
        {
            if (!string.IsNullOrWhiteSpace(participantId))
            {
                conversation.Participants.Add(new ConversationParticipant 
                { 
                    ConversationId = conversation.ConversationId, 
                    UserId = participantId,
                    JoinedAt = DateTime.UtcNow
                });
            }
        }

        return conversation;
    }

    #endregion

    #region Domain Methods

    /// <summary>
    /// Updates the conversation when a new message is received
    /// This method ensures consistency of conversation state
    /// </summary>
    public void UpdateLastMessage(string messagePreview)
    {
        LastMessageAt = DateTime.UtcNow;
        LastMessagePreview = messagePreview?.Length > 100 
            ? messagePreview.Substring(0, 97) + "..." 
            : messagePreview;
    }

    /// <summary>
    /// Adds a participant to a group conversation
    /// Works with the Participants navigation property
    /// </summary>
    public bool AddParticipant(string userId)
    {
        if (Type != ConversationType.Group)
            throw new InvalidOperationException("Can only add participants to group conversations");
        
        if (string.IsNullOrWhiteSpace(userId))
            return false;
        
        if (Participants.Any(p => p.UserId == userId))
            return false; // Already a participant
        
        Participants.Add(new ConversationParticipant 
        { 
            ConversationId = ConversationId, 
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        });
        return true;
    }

    /// <summary>
    /// Removes a participant from a group conversation
    /// Ensures at least 2 participants remain
    /// </summary>
    public bool RemoveParticipant(string userId)
    {
        if (Type != ConversationType.Group)
            throw new InvalidOperationException("Can only remove participants from group conversations");
        
        if (Participants.Count <= 2)
            throw new InvalidOperationException("Group conversations must have at least 2 participants");
        
        var participant = Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
        {
            Participants.Remove(participant);
            return true;
        }
        
        return false;
    }

    #endregion
}
