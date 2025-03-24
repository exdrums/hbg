namespace API.Contacts.Model;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a message in a conversation
/// </summary>
[Table("Messages")]
[Index(nameof(ConversationId), nameof(Timestamp))]
[Index(nameof(AuthorId))]
public class Message
{
    /// <summary>
    /// Unique identifier for the message
    /// </summary>
    [Key]
    [Required]
    [Column("Id", TypeName = "varchar(36)")]
    public string Id { get; private set; }

    /// <summary>
    /// ID of the conversation this message belongs to
    /// </summary>
    [Required]
    [Column("ConversationId", TypeName = "varchar(36)")]
    public string ConversationId { get; private set; }

    /// <summary>
    /// ID of the user who sent this message
    /// </summary>
    [Required]
    [Column("AuthorId", TypeName = "varchar(36)")]
    public string AuthorId { get; private set; }

    /// <summary>
    /// Content of the message
    /// </summary>
    [Required]
    [Column("Text", TypeName = "text")]
    public string Text { get; private set; }

    /// <summary>
    /// Timestamp when the message was sent
    /// </summary>
    [Required]
    [Column("Timestamp", TypeName = "timestamp with time zone")]
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// If the message was edited
    /// </summary>
    [Required]
    [Column("IsEdited", TypeName = "boolean")]
    public bool IsEdited { get; private set; }
    
    /// <summary>
    /// If this is a system alert message
    /// </summary>
    [Required]
    [Column("IsSystemAlert", TypeName = "boolean")]
    public bool IsSystemAlert { get; private set; }

    /// <summary>
    /// For AI messages that are being regenerated
    /// </summary>
    [Required]
    [Column("IsBeingRegenerated", TypeName = "boolean")]
    public bool IsBeingRegenerated { get; private set; }

    /// <summary>
    /// Optional reference to a parent message (for threaded replies)
    /// </summary>
    [Column("ParentMessageId", TypeName = "varchar(36)")]
    public string ParentMessageId { get; private set; }

    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; private set; }

    [ForeignKey(nameof(AuthorId))]
    public User Author { get; private set; }

    [ForeignKey(nameof(ParentMessageId))]
    public Message ParentMessage { get; private set; }

    public Message(string id, string conversationId, string authorId, string text, DateTime timestamp, string parentMessageId = null)
    {
        Id = id;
        ConversationId = conversationId;
        AuthorId = authorId;
        Text = text;
        Timestamp = timestamp;
        IsEdited = false;
        IsSystemAlert = false;
        IsBeingRegenerated = false;
        ParentMessageId = parentMessageId;
    }

    public void MarkAsRegenerated() => IsBeingRegenerated = true;

    public void EditText(string newText)
    {
        Text = newText;
        IsEdited = true;
    }

    public void CompleteRegeneration(string newText)
    {
        Text = newText;
        IsBeingRegenerated = false;
        IsEdited = true;
    }

    public static Message CreateSystemAlert(string conversationId, string alertText)
    {
        var message = new Message(
            Guid.NewGuid().ToString(),
            conversationId,
            "system",
            alertText,
            DateTime.UtcNow
        );
        message.IsSystemAlert = true;
        return message;
    }
}
