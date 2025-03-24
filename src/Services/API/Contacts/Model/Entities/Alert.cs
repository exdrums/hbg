namespace API.Contacts.Model;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a system alert
/// </summary>
[Table("Alerts")]
[Index(nameof(UserId))]
[Index(nameof(ConversationId))]
[Index(nameof(ExpiresAt))]
public class Alert
{
    /// <summary>
    /// Unique identifier for the alert
    /// </summary>
    [Key]
    [Required]
    [Column("Id", TypeName = "varchar(36)")]
    public string Id { get; private set; }

    /// <summary>
    /// The alert message
    /// </summary>
    [Required]
    [Column("Message", TypeName = "text")]
    public string Message { get; private set; }

    /// <summary>
    /// When the alert was created
    /// </summary>
    [Required]
    [Column("CreatedAt", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the alert expires
    /// </summary>
    [Column("ExpiresAt", TypeName = "timestamp with time zone")]
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// User ID this alert is for (null if global)
    /// </summary>
    [Column("UserId", TypeName = "varchar(36)")]
    public string UserId { get; private set; }

    /// <summary>
    /// Conversation ID this alert is for (null if global)
    /// </summary>
    [Column("ConversationId", TypeName = "varchar(36)")]
    public string ConversationId { get; private set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; private set; }

    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; private set; }

    public Alert(string id, string message, string userId = null, string conversationId = null, DateTime? expiresAt = null)
    {
        Id = id;
        Message = message;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        UserId = userId;
        ConversationId = conversationId;
    }

    public bool IsExpired() => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

}