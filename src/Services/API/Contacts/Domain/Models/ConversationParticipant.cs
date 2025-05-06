using System;

namespace API.Contacts.Domain.Models
{
    /// <summary>
    /// Represents a participant in a conversation
    /// </summary>
    public class ConversationParticipant
    {
        /// <summary>
        /// ID of the conversation this participant belongs to
        /// </summary>
        public string ConversationId { get; private set; }

        /// <summary>
        /// ID of the user who is a participant
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Reference to the user entity
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Role of the participant in the conversation
        /// </summary>
        public ParticipantRole Role { get; private set; }

        /// <summary>
        /// Timestamp of when the participant joined the conversation
        /// </summary>
        public DateTime JoinedAt { get; private set; }

        /// <summary>
        /// Timestamp of when the participant last read the conversation
        /// </summary>
        public DateTime LastReadAt { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private ConversationParticipant() { }

        /// <summary>
        /// Creates a new conversation participant
        /// </summary>
        public ConversationParticipant(string conversationId, string userId, ParticipantRole role = ParticipantRole.Member)
        {
            ConversationId = conversationId ?? throw new ArgumentNullException(nameof(conversationId));
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Role = role;
            JoinedAt = DateTime.UtcNow;
            LastReadAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the role of the participant
        /// </summary>
        public void UpdateRole(ParticipantRole newRole)
        {
            Role = newRole;
        }

        /// <summary>
        /// Updates the timestamp of when the participant last read the conversation
        /// </summary>
        public void UpdateLastRead(DateTime timestamp)
        {
            // Ensure we don't move backwards in time
            if (timestamp > LastReadAt)
            {
                LastReadAt = timestamp;
            }
        }
    }
}
