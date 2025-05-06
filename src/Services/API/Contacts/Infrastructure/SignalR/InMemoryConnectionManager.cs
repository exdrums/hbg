using API.Contacts.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace API.Contacts.Infrastructure.SignalR
{
    /// <summary>
    /// In-memory implementation of the connection manager.
    /// In a production environment, this should be replaced with a distributed cache implementation (e.g., Redis)
    /// for proper scalability across multiple instances.
    /// </summary>
    public class InMemoryConnectionManager : IConnectionManager
    {
        private readonly Dictionary<string, string> _connectionToUser = new();
        private readonly Dictionary<string, List<string>> _userToConnections = new();
        private readonly Dictionary<string, List<string>> _conversationToConnections = new();
        private readonly Dictionary<string, List<string>> _connectionToConversations = new();

        private readonly object _lock = new();

        /// <summary>
        /// Adds a connection for a user
        /// </summary>
        public void AddConnection(string connectionId, string userId)
        {
            lock (_lock)
            {
                _connectionToUser[connectionId] = userId;

                if (!_userToConnections.ContainsKey(userId))
                {
                    _userToConnections[userId] = new List<string>();
                }

                _userToConnections[userId].Add(connectionId);
            }
        }

        /// <summary>
        /// Removes a connection
        /// </summary>
        public void RemoveConnection(string connectionId)
        {
            lock (_lock)
            {
                if (_connectionToUser.TryGetValue(connectionId, out var userId))
                {
                    _connectionToUser.Remove(connectionId);

                    if (_userToConnections.ContainsKey(userId))
                    {
                        _userToConnections[userId].Remove(connectionId);

                        if (!_userToConnections[userId].Any())
                        {
                            _userToConnections.Remove(userId);
                        }
                    }
                }

                if (_connectionToConversations.TryGetValue(connectionId, out var conversations))
                {
                    foreach (var conversationId in conversations.ToList())
                    {
                        RemoveFromConversation(connectionId, conversationId);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a connection to a conversation group
        /// </summary>
        public void AddToConversation(string connectionId, string conversationId)
        {
            lock (_lock)
            {
                if (!_conversationToConnections.ContainsKey(conversationId))
                {
                    _conversationToConnections[conversationId] = new List<string>();
                }

                if (!_conversationToConnections[conversationId].Contains(connectionId))
                {
                    _conversationToConnections[conversationId].Add(connectionId);
                }

                if (!_connectionToConversations.ContainsKey(connectionId))
                {
                    _connectionToConversations[connectionId] = new List<string>();
                }

                if (!_connectionToConversations[connectionId].Contains(conversationId))
                {
                    _connectionToConversations[connectionId].Add(conversationId);
                }
            }
        }

        /// <summary>
        /// Removes a connection from a conversation group
        /// </summary>
        public void RemoveFromConversation(string connectionId, string conversationId)
        {
            lock (_lock)
            {
                if (_conversationToConnections.ContainsKey(conversationId))
                {
                    _conversationToConnections[conversationId].Remove(connectionId);

                    if (!_conversationToConnections[conversationId].Any())
                    {
                        _conversationToConnections.Remove(conversationId);
                    }
                }

                if (_connectionToConversations.ContainsKey(connectionId))
                {
                    _connectionToConversations[connectionId].Remove(conversationId);

                    if (!_connectionToConversations[connectionId].Any())
                    {
                        _connectionToConversations.Remove(connectionId);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all connection IDs for a user
        /// </summary>
        public IEnumerable<string> GetConnectionsForUser(string userId)
        {
            lock (_lock)
            {
                return _userToConnections.TryGetValue(userId, out var connections)
                    ? connections.ToList()
                    : Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets all connection IDs for a conversation
        /// </summary>
        public IEnumerable<string> GetConnectionsForConversation(string conversationId)
        {
            lock (_lock)
            {
                return _conversationToConnections.TryGetValue(conversationId, out var connections)
                    ? connections.ToList()
                    : Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the user ID associated with a connection
        /// </summary>
        public string GetUserIdForConnection(string connectionId)
        {
            lock (_lock)
            {
                return _connectionToUser.TryGetValue(connectionId, out var userId)
                    ? userId
                    : null;
            }
        }

        /// <summary>
        /// Gets all conversation IDs a connection is subscribed to
        /// </summary>
        public IEnumerable<string> GetConversationsForConnection(string connectionId)
        {
            lock (_lock)
            {
                return _connectionToConversations.TryGetValue(connectionId, out var conversations)
                    ? conversations.ToList()
                    : Enumerable.Empty<string>();
            }
        }
    }
}
