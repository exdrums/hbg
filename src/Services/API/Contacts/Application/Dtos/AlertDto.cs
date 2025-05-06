using API.Contacts.Domain.Models;
using System;

namespace API.Contacts.Application.Dtos
{
    /// <summary>
    /// DTO for Alert entity
    /// </summary>
    public class AlertDto
    {
        /// <summary>
        /// Alert identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of alert
        /// </summary>
        public AlertType Type { get; set; }

        /// <summary>
        /// Content text of the alert
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Timestamp when the alert was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Whether the alert has been read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Related conversation ID (optional)
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Related message ID (optional)
        /// </summary>
        public string MessageId { get; set; }
    }
}
