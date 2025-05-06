namespace API.Contacts.Domain.Models
{
    /// <summary>
    /// Enum representing the role of a participant in a conversation
    /// </summary>
    public enum ParticipantRole
    {
        /// <summary>
        /// A regular participant in the conversation
        /// </summary>
        Member = 0,

        /// <summary>
        /// An administrator of the conversation (can add/remove members)
        /// </summary>
        Admin = 1,

        /// <summary>
        /// The AI assistant in an AI assistant conversation
        /// </summary>
        AiAssistant = 2
    }
}
