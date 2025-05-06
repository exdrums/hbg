namespace API.Contacts.Domain.Models
{
    /// <summary>
    /// Enum representing the type of conversation
    /// </summary>
    public enum ConversationType
    {
        /// <summary>
        /// A one-on-one conversation between two users
        /// </summary>
        OneOnOne = 0,

        /// <summary>
        /// A group conversation with multiple participants
        /// </summary>
        Group = 1,

        /// <summary>
        /// A conversation with an AI assistant
        /// </summary>
        AiAssistant = 2
    }
}
