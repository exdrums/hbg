using System;

namespace API.Contacts.Domain.Models
{
    /// <summary>
    /// Represents a message in a conversation
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Unique identifier for the message
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// ID of the conversation this message belongs to
        /// </summary>
        public string ConversationId { get; private set; }

        /// <summary>
        /// ID of the user who authored the message
        /// </summary>
        public string AuthorId { get; private set; }

        /// <summary>
        /// Reference to the author entity
        /// </summary>
        public User Author { get; private set; }

        /// <summary>
        /// Content text of the message
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Timestamp when the message was sent
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Whether the message has been edited
        /// </summary>
        public bool IsEdited { get; private set; }

        /// <summary>
        /// Whether this is a system-generated alert message
        /// </summary>
        public bool IsSystemAlert { get; private set; }

        /// <summary>
        /// Whether this message is currently being regenerated (for AI messages)
        /// </summary>
        public bool IsBeingRegenerated { get; private set; }

        /// <summary>
        /// Optional ID of the parent message (for replies/threads)
        /// </summary>
        public string ParentMessageId { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private Message() { }

        /// <summary>
        /// Creates a new message
        /// </summary>
        public Message(string id, string conversationId, string authorId, string text, DateTime timestamp, string parentMessageId = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ConversationId = conversationId ?? throw new ArgumentNullException(nameof(conversationId));
            AuthorId = authorId ?? throw new ArgumentNullException(nameof(authorId));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Timestamp = timestamp;
            ParentMessageId = parentMessageId;
            IsEdited = false;
            IsSystemAlert = false;
            IsBeingRegenerated = false;
        }

        /// <summary>
        /// Creates a system alert message
        /// </summary>
        public static Message CreateSystemAlert(string conversationId, string alertText)
        {
            if (string.IsNullOrEmpty(conversationId)) throw new ArgumentNullException(nameof(conversationId));
            if (string.IsNullOrEmpty(alertText)) throw new ArgumentNullException(nameof(alertText));

            var id = Guid.NewGuid().ToString();
            var message = new Message(id, conversationId, "system", alertText, DateTime.UtcNow);
            message.IsSystemAlert = true;

            return message;
        }

        /// <summary>
        /// Edits the text of the message
        /// </summary>
        public void EditText(string newText)
        {
            if (string.IsNullOrEmpty(newText)) throw new ArgumentNullException(nameof(newText));
            if (IsSystemAlert) throw new InvalidOperationException("Cannot edit a system alert message");
            if (IsBeingRegenerated) throw new InvalidOperationException("Cannot edit a message that is being regenerated");

            Text = newText;
            IsEdited = true;
        }

        /// <summary>
        /// Marks this message as being regenerated (for AI messages)
        /// </summary>
        public void MarkAsRegenerated()
        {
            IsBeingRegenerated = true;
        }

        /// <summary>
        /// Completes regeneration with the new text (for AI messages)
        /// </summary>
        public void CompleteRegeneration(string newText)
        {
            if (string.IsNullOrEmpty(newText)) throw new ArgumentNullException(nameof(newText));

            Text = newText;
            IsBeingRegenerated = false;
            IsEdited = true;
        }
    }
}
