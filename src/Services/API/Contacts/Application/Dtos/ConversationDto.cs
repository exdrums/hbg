using API.Contacts.Domain.Models;
using System;
using System.Collections.Generic;

namespace API.Contacts.Application.Dtos
{
    /// <summary>
    /// DTO for Conversation entity
    /// </summary>
    public class ConversationDto
    {
        /// <summary>
        /// Conversation identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title or name of the conversation
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Type of the conversation
        /// </summary>
        public ConversationType Type { get; set; }

        /// <summary>
        /// List of participants in the conversation
        /// </summary>
        public List<UserDto> Participants { get; set; }

        /// <summary>
        /// Timestamp of the most recent message
        /// </summary>
        public DateTime LastMessageAt { get; set; }

        /// <summary>
        /// Whether the conversation is archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Count of unread messages (optional, calculated at runtime)
        /// </summary>
        public int? UnreadCount { get; set; }

        /// <summary>
        /// Most recent message preview (optional)
        /// </summary>
        public MessageDto LastMessage { get; set; }
    }
}
