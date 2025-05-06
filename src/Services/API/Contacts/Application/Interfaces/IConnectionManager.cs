namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Interface for managing real-time connections
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Adds a connection for a user
        /// </summary>
        void AddConnection(string connectionId, string userId);

        /// <summary>
        /// Removes a connection
        /// </summary>
        void RemoveConnection(string connectionId);

        /// <summary>
        /// Adds a connection to a conversation group
        /// </summary>
        void AddToConversation(string connectionId, string conversationId);

        /// <summary>
        /// Removes a connection from a conversation group
        /// </summary>
        void RemoveFromConversation(string connectionId, string conversationId);

        /// <summary>
        /// Gets all connection IDs for a user
        /// </summary>
        IEnumerable<string> GetConnectionsForUser(string userId);

        /// <summary>
        /// Gets all connection IDs for a conversation
        /// </summary>
        IEnumerable<string> GetConnectionsForConversation(string conversationId);

        /// <summary>
        /// Gets the user ID associated with a connection
        /// </summary>
        string GetUserIdForConnection(string connectionId);

        /// <summary>
        /// Gets all conversation IDs a connection is subscribed to
        /// </summary>
        IEnumerable<string> GetConversationsForConnection(string connectionId);
    }
}
