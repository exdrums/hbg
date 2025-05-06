using System;

namespace API.Contacts.Application.Dtos
{
    /// <summary>
    /// DTO for Message entity
    /// </summary>
    public class MessageDto
    {
        /// <summary>
        /// Message identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Content text of the message
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Timestamp when the message was sent
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Author of the message
        /// </summary>
        public UserDto Author { get; set; }

        /// <summary>
        /// Whether the message has been edited
        /// </summary>
        public bool IsEdited { get; set; }

        /// <summary>
        /// Whether the message is being regenerated (for AI messages)
        /// </summary>
        public bool IsBeingRegenerated { get; set; }

        /// <summary>
        /// Whether the message is a system alert
        /// </summary>
        public bool IsSystemAlert { get; set; }

        /// <summary>
        /// Optional ID of the parent message (for replies/threads)
        /// </summary>
        public string ParentMessageId { get; set; }
    }
}
