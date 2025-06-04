using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Models;

/// <summary>
/// Message entity with EF Core mapping attributes
/// Messages are designed to be immutable after creation (except for specific operations)
/// 
/// Database Design Considerations:
/// 1. Messages are never hard-deleted (audit trail requirement)
/// 2. Content is stored as NVARCHAR(MAX) for Unicode support
/// 3. Indexes optimize for common query patterns
/// 4. Read receipts are tracked through a separate join table
/// </summary>
[Table("Messages")]
[Index(nameof(ConversationId), nameof(SentAt))] // For loading conversation messages
[Index(nameof(SenderUserId))] // For finding user's messages
[Index(nameof(Type))] // For filtering by message type
public class Message : IMessage
{
    /// <summary>
    /// EF Core requires a parameterless constructor
    /// Initialize collections to prevent null reference issues
    /// </summary>
    public Message()
    {
        ReadReceipts = new HashSet<MessageReadReceipt>();
        SentAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    /// <summary>
    /// Primary key for the message
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] // We generate GUIDs
    public Guid MessageId { get; set; }

    /// <summary>
    /// Foreign key to the conversation
    /// Required for referential integrity
    /// </summary>
    [Required]
    public Guid ConversationId { get; set; }

    /// <summary>
    /// The user who sent the message
    /// "SYSTEM" for system-generated messages
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SenderUserId { get; set; }

    /// <summary>
    /// The message content
    /// Using MAX length for rich content support
    /// Content is replaced with "Message deleted" when deleted
    /// </summary>
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// Type of message determines rendering and behavior
    /// </summary>
    [Required]
    public MessageType Type { get; set; }

    /// <summary>
    /// When the message was sent
    /// Immutable after creation
    /// </summary>
    [Required]
    public DateTime SentAt { get; set; }

    /// <summary>
    /// When the message was last edited
    /// Null if never edited
    /// </summary>
    public DateTime? EditedAt { get; set; }

    /// <summary>
    /// Soft delete flag
    /// When true, Content should show "Message deleted"
    /// </summary>
    [Required]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// If this is a reply, the message being replied to
    /// Creates threaded conversation support
    /// </summary>
    public Guid? ReplyToMessageId { get; set; }

    /// <summary>
    /// JSON metadata for special message types
    /// Stores file info, location data, etc.
    /// </summary>
    public string Metadata { get; set; }

    /// <summary>
    /// Navigation property to the conversation
    /// </summary>
    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; }

    /// <summary>
    /// Navigation property for read receipts
    /// Many-to-many relationship with users through MessageReadReceipt
    /// </summary>
    public virtual ICollection<MessageReadReceipt> ReadReceipts { get; set; }

    /// <summary>
    /// Navigation property for the message being replied to
    /// Self-referencing foreign key
    /// </summary>
    [ForeignKey(nameof(ReplyToMessageId))]
    public virtual Message ReplyToMessage { get; set; }

    #region IMessage Implementation

    /// <summary>
    /// Computed property for interface compatibility
    /// Extracts user IDs from read receipts
    /// </summary>
    [NotMapped]
    public ICollection<string> ReadByUserIds
    {
        get => ReadReceipts?.Select(r => r.UserId).ToList() ?? new List<string>();
        set => throw new NotSupportedException("Use ReadReceipts navigation property");
    }

    #endregion

    #region Factory Methods (Same as before, updated for EF)

    /// <summary>
    /// Factory method for creating a text message
    /// </summary>
    public static Message CreateTextMessage(
        Guid conversationId, 
        string senderUserId, 
        string content,
        Guid? replyToMessageId = null)
    {
        ValidateCommonParameters(conversationId, senderUserId);
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty", nameof(content));

        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderUserId = senderUserId,
            Content = content.Trim(),
            Type = MessageType.Text,
            ReplyToMessageId = replyToMessageId
        };

        // The sender has automatically read their own message
        message.ReadReceipts.Add(new MessageReadReceipt
        {
            MessageId = message.MessageId,
            UserId = senderUserId,
            ReadAt = DateTime.UtcNow
        });

        return message;
    }

    /// <summary>
    /// Factory method for creating a system message
    /// </summary>
    public static Message CreateSystemMessage(Guid conversationId, string content)
    {
        if (conversationId == Guid.Empty)
            throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("System message content cannot be empty", nameof(content));

        return new Message
        {
            MessageId = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderUserId = "SYSTEM",
            Content = content,
            Type = MessageType.System
        };
    }

    /// <summary>
    /// Factory method for creating a file message
    /// </summary>
    public static Message CreateFileMessage(
        Guid conversationId,
        string senderUserId,
        string fileName,
        long fileSize,
        string fileUrl,
        string mimeType)
    {
        ValidateCommonParameters(conversationId, senderUserId);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        
        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentException("File URL cannot be empty", nameof(fileUrl));

        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderUserId = senderUserId,
            Content = fileName,
            Type = MessageType.File,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                fileName,
                fileSize,
                fileUrl,
                mimeType
            })
        };

        message.ReadReceipts.Add(new MessageReadReceipt
        {
            MessageId = message.MessageId,
            UserId = senderUserId,
            ReadAt = DateTime.UtcNow
        });

        return message;
    }

    /// <summary>
    /// Creates an alert message (non-persistent notification)
    /// </summary>
    public static Message CreateAlertMessage(
        Guid conversationId,
        string content,
        string alertType = "info")
    {
        if (conversationId == Guid.Empty)
            throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Alert content cannot be empty", nameof(content));

        return new Message
        {
            MessageId = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderUserId = "SYSTEM",
            Content = content,
            Type = MessageType.Alert,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                alertType,
                isPersistent = false
            })
        };
    }

    #endregion

    #region Domain Methods

    /// <summary>
    /// Marks the message as read by a specific user
    /// </summary>
    public bool MarkAsReadBy(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (ReadReceipts.Any(r => r.UserId == userId))
            return false; // Already read

        ReadReceipts.Add(new MessageReadReceipt
        {
            MessageId = MessageId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        });

        return true;
    }

    /// <summary>
    /// Edits the content of an existing message
    /// </summary>
    public bool Edit(string newContent, string editorUserId)
    {
        if (Type == MessageType.System || Type == MessageType.Alert)
            return false;
        
        if (IsDeleted)
            return false;
        
        if (SenderUserId != editorUserId)
            return false;
        
        if (string.IsNullOrWhiteSpace(newContent))
            return false;

        Content = newContent.Trim();
        EditedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Marks the message as deleted
    /// </summary>
    public bool Delete(string deleterUserId)
    {
        if (Type == MessageType.System)
            return false;
        
        if (IsDeleted)
            return false;
        
        if (SenderUserId != deleterUserId)
            return false;

        IsDeleted = true;
        Content = "Message deleted";
        EditedAt = DateTime.UtcNow;
        Metadata = null;
        return true;
    }

    private static void ValidateCommonParameters(Guid conversationId, string senderUserId)
    {
        if (conversationId == Guid.Empty)
            throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));
        
        if (string.IsNullOrWhiteSpace(senderUserId))
            throw new ArgumentException("Sender user ID cannot be empty", nameof(senderUserId));
    }

    #endregion
}
