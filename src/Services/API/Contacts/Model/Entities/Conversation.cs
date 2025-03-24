namespace API.Contacts.Model;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a chat conversation
/// </summary>
[Table("Conversations")]
[Index(nameof(LastMessageAt))]
public class Conversation
{
    /// <summary>
    /// Unique identifier for the conversation
    /// </summary>
    [Key]
    [Required]
    [Column("Id", TypeName = "varchar(36)")]
    public string Id { get; private set; }

    /// <summary>
    /// Title of the conversation
    /// </summary>
    [Column("Title", TypeName = "varchar(200)")]
    public string Title { get; private set; }

    /// <summary>
    /// Type of the conversation (User-to-User or AI Assistant)
    /// </summary>
    [Required]
    [Column("Type", TypeName = "int")]
    public ConversationType Type { get; private set; }

    /// <summary>
    /// Collection of users participating in this conversation
    /// </summary>
    public ICollection<ConversationParticipant> Participants { get; private set; }

    /// <summary>
    /// Timestamp of when the conversation was created
    /// </summary>
    [Required]
    [Column("CreatedAt", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp of the last message in the conversation
    /// </summary>
    [Required]
    [Column("LastMessageAt", TypeName = "timestamp with time zone")]
    public DateTime LastMessageAt { get; private set; }

    /// <summary>
    /// Whether this conversation is archived
    /// </summary>
    [Required]
    [Column("IsArchived", TypeName = "boolean")]
    public bool IsArchived { get; private set; }

    public Conversation(string id, string title, ConversationType type)
    {
        Id = id;
        Title = title;
        Type = type;
        Participants = new List<ConversationParticipant>();
        CreatedAt = DateTime.UtcNow;
        LastMessageAt = CreatedAt;
        IsArchived = false;
    }

    public void AddParticipant(User user, ParticipantRole role)
    {
        if (Participants.Any(p => p.UserId == user.Id))
            return;

        var participant = new ConversationParticipant(Id, user.Id, role);
        Participants.Add(participant);
    }

    public void RemoveParticipant(string userId)
    {
        var participant = Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
        {
            Participants.Remove(participant);
        }
    }

    public void UpdateLastMessageTime(DateTime timestamp)
    {
        LastMessageAt = timestamp;
    }

    public void ArchiveConversation()
    {
        IsArchived = true;
    }

    public void RestoreConversation()
    {
        IsArchived = false;
    }

    public void UpdateTitle(string newTitle)
    {
        Title = newTitle;
    }
}
