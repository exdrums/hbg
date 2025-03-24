namespace API.Contacts.Model;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a participant in a conversation
/// </summary>
[Table("ConversationParticipants")]
[Index(nameof(UserId))]
public class ConversationParticipant
{
    /// <summary>
    /// ID of the conversation
    /// </summary>
    [Required]
    [Column("ConversationId", TypeName = "varchar(36)")]
    public string ConversationId { get; private set; }

    /// <summary>
    /// ID of the user
    /// </summary>
    [Required]
    [Column("UserId", TypeName = "varchar(36)")]
    public string UserId { get; private set; }

    /// <summary>
    /// Role of the participant in the conversation
    /// </summary>
    [Required]
    [Column("Role", TypeName = "int")]
    public ParticipantRole Role { get; private set; }

    /// <summary>
    /// If this participant is currently typing
    /// </summary>
    [Required]
    [Column("IsTyping", TypeName = "boolean")]
    public bool IsTyping { get; private set; }

    /// <summary>
    /// When this participant started typing
    /// </summary>
    [Column("TypingStartTime", TypeName = "timestamp with time zone")]
    public DateTime? TypingStartTime { get; private set; }

    /// <summary>
    /// Last time when read receipts were sent
    /// </summary>
    [Required]
    [Column("LastReadAt", TypeName = "timestamp with time zone")]
    public DateTime LastReadAt { get; private set; }

    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; private set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; private set; }

    public ConversationParticipant(string conversationId, string userId, ParticipantRole role)
    {
        ConversationId = conversationId;
        UserId = userId;
        Role = role;
        IsTyping = false;
        TypingStartTime = null;
        LastReadAt = DateTime.UtcNow;
    }

    public void StartTyping()
    {
        IsTyping = true;
        TypingStartTime = DateTime.UtcNow;
    }

    public void StopTyping()
    {
        IsTyping = false;
        TypingStartTime = null;
    }

    public void UpdateLastRead(DateTime timestamp)
    {
        LastReadAt = timestamp;
    }

    public void ChangeRole(ParticipantRole newRole)
    {
        Role = newRole;
    }
}