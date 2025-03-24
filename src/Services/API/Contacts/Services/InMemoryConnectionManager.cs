using API.Contacts.Services.Interfaces;

namespace API.Contacts.Services;

/// <summary>
/// In-memory implementation of the connection manager
/// In a production environment, this would use a distributed cache like Redis
/// </summary>
public class InMemoryConnectionManager : IConnectionManager
{
    private readonly Dictionary<string, string> _connectionToUser = new();
    private readonly Dictionary<string, List<string>> _userToConnections = new();
    private readonly Dictionary<string, List<string>> _conversationToConnections = new();
    private readonly Dictionary<string, List<string>> _connectionToConversations = new();

    private readonly object _lock = new();

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
                foreach (var conversationId in conversations)
                {
                    if (_conversationToConnections.ContainsKey(conversationId))
                    {
                        _conversationToConnections[conversationId].Remove(connectionId);

                        if (!_conversationToConnections[conversationId].Any())
                        {
                            _conversationToConnections.Remove(conversationId);
                        }
                    }
                }

                _connectionToConversations.Remove(connectionId);
            }
        }
    }

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

    public IEnumerable<string> GetConnectionsForUser(string userId)
    {
        lock (_lock)
        {
            return _userToConnections.TryGetValue(userId, out var connections)
                ? connections.ToList()
                : Enumerable.Empty<string>();
        }
    }

    public IEnumerable<string> GetConnectionsForConversation(string conversationId)
    {
        lock (_lock)
        {
            return _conversationToConnections.TryGetValue(conversationId, out var connections)
                ? connections.ToList()
                : Enumerable.Empty<string>();
        }
    }
}

