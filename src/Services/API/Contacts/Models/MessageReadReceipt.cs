using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Models;

/// <summary>
/// Tracks which users have read which messages
/// This is a join entity for the many-to-many relationship
/// 
/// Read Receipt Design:
/// 1. Provides "seen by" functionality in group chats
/// 2. Enables single/double checkmark indicators
/// 3. Helps calculate unread counts efficiently
/// 4. Privacy consideration: users might want to disable read receipts
/// 
/// Performance optimization: This table can grow very large
/// Consider archiving old receipts or using a time-series database
/// </summary>
[Table("MessageReadReceipts")]
[Index(nameof(MessageId), nameof(UserId), IsUnique = true)] // Prevent duplicate reads
[Index(nameof(UserId), nameof(ReadAt))] // For "last seen" queries
public class MessageReadReceipt
{
    /// <summary>
    /// The message that was read
    /// Part of composite primary key
    /// </summary>
    [Required]
    public Guid MessageId { get; set; }

    /// <summary>
    /// The user who read the message
    /// Part of composite primary key
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UserId { get; set; }

    /// <summary>
    /// When the message was read
    /// Used for "seen at" timestamps
    /// </summary>
    [Required]
    public DateTime ReadAt { get; set; }

    /// <summary>
    /// Navigation property to the message
    /// </summary>
    [ForeignKey(nameof(MessageId))]
    public virtual Message Message { get; set; }

    /// <summary>
    /// Note: No navigation to User as users are external
    /// </summary>
}
