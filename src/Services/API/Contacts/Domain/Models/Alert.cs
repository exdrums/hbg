using System;

namespace API.Contacts.Domain.Models
{
    /// <summary>
    /// Type of alert
    /// </summary>
    public enum AlertType
    {
        /// <summary>
        /// System notification
        /// </summary>
        System = 0,

        /// <summary>
        /// New message notification
        /// </summary>
        NewMessage = 1,

        /// <summary>
        /// Mention notification
        /// </summary>
        Mention = 2,

        /// <summary>
        /// Request notification
        /// </summary>
        Request = 3
    }

    /// <summary>
    /// Represents an alert or notification for a user
    /// </summary>
    public class Alert
    {
        /// <summary>
        /// Unique identifier for the alert
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// ID of the user this alert is for
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Optional ID of the related conversation
        /// </summary>
        public string ConversationId { get; private set; }

        /// <summary>
        /// Optional ID of the related message
        /// </summary>
        public string MessageId { get; private set; }

        /// <summary>
        /// Type of alert
        /// </summary>
        public AlertType Type { get; private set; }

        /// <summary>
        /// Content text of the alert
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Timestamp when the alert was created
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Whether the alert has been read by the user
        /// </summary>
        public bool IsRead { get; private set; }

        /// <summary>
        /// Whether the alert has been dismissed by the user
        /// </summary>
        public bool IsDismissed { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private Alert() { }

        /// <summary>
        /// Creates a new alert
        /// </summary>
        public Alert(string id, string userId, AlertType type, string text, string conversationId = null, string messageId = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Type = type;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            ConversationId = conversationId;
            MessageId = messageId;
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
            IsDismissed = false;
        }

        /// <summary>
        /// Creates a new message alert
        /// </summary>
        public static Alert CreateMessageAlert(string userId, string conversationId, string messageId, string authorName)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(conversationId)) throw new ArgumentNullException(nameof(conversationId));
            if (string.IsNullOrEmpty(messageId)) throw new ArgumentNullException(nameof(messageId));
            if (string.IsNullOrEmpty(authorName)) throw new ArgumentNullException(nameof(authorName));

            var id = Guid.NewGuid().ToString();
            var text = $"New message from {authorName}";

            return new Alert(id, userId, AlertType.NewMessage, text, conversationId, messageId);
        }

        /// <summary>
        /// Creates a new system alert
        /// </summary>
        public static Alert CreateSystemAlert(string userId, string text)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            var id = Guid.NewGuid().ToString();

            return new Alert(id, userId, AlertType.System, text);
        }

        /// <summary>
        /// Marks the alert as read
        /// </summary>
        public void MarkAsRead()
        {
            IsRead = true;
        }

        /// <summary>
        /// Dismisses the alert
        /// </summary>
        public void Dismiss()
        {
            IsDismissed = true;
        }
    }
}
