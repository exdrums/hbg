using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Contacts.Domain.Models
{
    /// <summary>
    /// Represents a conversation between users in the chat system
    /// </summary>
    public class Conversation
    {
        private readonly List<ConversationParticipant> _participants = new();

        /// <summary>
        /// Unique identifier for the conversation
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Title or name of the conversation
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Type of the conversation (OneOnOne, Group, AiAssistant)
        /// </summary>
        public ConversationType Type { get; private set; }

        /// <summary>
        /// List of participants in the conversation
        /// </summary>
        public IReadOnlyCollection<ConversationParticipant> Participants => _participants.AsReadOnly();

        /// <summary>
        /// Timestamp of the most recent message in the conversation
        /// </summary>
        public DateTime LastMessageAt { get; private set; }

        /// <summary>
        /// Timestamp when the conversation was created
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Whether the conversation is archived
        /// </summary>
        public bool IsArchived { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private Conversation() { }

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        public Conversation(string id, string title, ConversationType type)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? string.Empty;
            Type = type;
            CreatedAt = DateTime.UtcNow;
            LastMessageAt = CreatedAt;
            IsArchived = false;
        }

        /// <summary>
        /// Adds a participant to the conversation
        /// </summary>
        public void AddParticipant(string userId, ParticipantRole role = ParticipantRole.Member)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            // Check if the user is already a participant
            if (_participants.Any(p => p.UserId == userId))
                return;

            var participant = new ConversationParticipant(Id, userId, role);
            _participants.Add(participant);
        }

        /// <summary>
        /// Removes a participant from the conversation
        /// </summary>
        public void RemoveParticipant(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            var participant = _participants.FirstOrDefault(p => p.UserId == userId);
            if (participant != null)
            {
                _participants.Remove(participant);
            }
        }

        /// <summary>
        /// Updates the timestamp of the most recent message
        /// </summary>
        public void UpdateLastMessageTime(DateTime timestamp)
        {
            // Ensure we don't move backwards in time
            if (timestamp > LastMessageAt)
            {
                LastMessageAt = timestamp;
            }
        }

        /// <summary>
        /// Updates the title of the conversation
        /// </summary>
        public void UpdateTitle(string title)
        {
            Title = title ?? string.Empty;
        }

        /// <summary>
        /// Archives or unarchives the conversation
        /// </summary>
        public void SetArchived(bool isArchived)
        {
            IsArchived = isArchived;
        }

        /// <summary>
        /// Creates a one-on-one conversation between two users
        /// </summary>
        public static Conversation CreateOneOnOne(string user1Id, string user2Id)
        {
            if (string.IsNullOrEmpty(user1Id)) throw new ArgumentNullException(nameof(user1Id));
            if (string.IsNullOrEmpty(user2Id)) throw new ArgumentNullException(nameof(user2Id));

            var id = Guid.NewGuid().ToString();
            var conversation = new Conversation(id, string.Empty, ConversationType.OneOnOne);

            conversation.AddParticipant(user1Id);
            conversation.AddParticipant(user2Id);

            return conversation;
        }

        /// <summary>
        /// Creates a group conversation with multiple participants
        /// </summary>
        public static Conversation CreateGroup(string creatorId, IEnumerable<string> participantIds, string title)
        {
            if (string.IsNullOrEmpty(creatorId)) throw new ArgumentNullException(nameof(creatorId));
            if (participantIds == null) throw new ArgumentNullException(nameof(participantIds));

            var id = Guid.NewGuid().ToString();
            var conversation = new Conversation(id, title, ConversationType.Group);

            // Add creator as admin
            conversation.AddParticipant(creatorId, ParticipantRole.Admin);

            // Add other participants
            foreach (var participantId in participantIds.Where(p => p != creatorId))
            {
                conversation.AddParticipant(participantId);
            }

            return conversation;
        }

        /// <summary>
        /// Creates an AI assistant conversation
        /// </summary>
        public static Conversation CreateAiAssistant(string userId, string aiAssistantId, string title)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(aiAssistantId)) throw new ArgumentNullException(nameof(aiAssistantId));

            var id = Guid.NewGuid().ToString();
            var conversation = new Conversation(id, title, ConversationType.AiAssistant);

            // Add user as participant
            conversation.AddParticipant(userId);

            // Add AI assistant as participant with AI role
            conversation.AddParticipant(aiAssistantId, ParticipantRole.AiAssistant);

            return conversation;
        }
    }
}
