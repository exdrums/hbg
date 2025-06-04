using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using API.Contacts.Services.Interfaces;

namespace API.Contacts.Services
{
    /// <summary>
    /// In-memory implementation of user connection tracking
    /// 
    /// Production Considerations:
    /// This implementation uses ConcurrentDictionary for thread-safety
    /// but is limited to a single server instance. For production:
    /// 
    /// 1. Replace with Redis for distributed caching across servers
    /// 2. Add connection expiration/TTL for cleanup
    /// 3. Implement connection heartbeat for accurate presence
    /// 4. Add metrics for monitoring connection health
    /// 
    /// The service maintains bidirectional mappings for efficiency:
    /// - User ID -> Connection IDs (for sending to user)
    /// - Connection ID -> User ID (for disconnect handling)
    /// </summary>
    public class UserConnectionService : IUserConnectionService
    {
        private readonly ILogger<UserConnectionService> _logger;
        
        // Thread-safe collections for connection tracking
        // In production, these would be replaced with Redis data structures
        private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections;
        private readonly ConcurrentDictionary<string, string> _connectionUsers;
        private readonly ConcurrentDictionary<string, DateTime> _userLastActivity;
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _connectionMetadata;
        
        // Lock objects for thread-safe HashSet operations
        private readonly ConcurrentDictionary<string, object> _userLocks;

        public UserConnectionService(ILogger<UserConnectionService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _userConnections = new ConcurrentDictionary<string, HashSet<string>>();
            _connectionUsers = new ConcurrentDictionary<string, string>();
            _userLastActivity = new ConcurrentDictionary<string, DateTime>();
            _connectionMetadata = new ConcurrentDictionary<string, Dictionary<string, string>>();
            _userLocks = new ConcurrentDictionary<string, object>();
        }

        #region Connection Management

        /// <summary>
        /// Registers a new connection for a user
        /// Thread-safe implementation handling multiple connections per user
        /// </summary>
        public async Task AddConnectionAsync(string userId, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Connection ID cannot be empty", nameof(connectionId));

            _logger.LogDebug($"Adding connection {connectionId} for user {userId}");

            // Get or create lock object for this user
            var userLock = _userLocks.GetOrAdd(userId, _ => new object());

            // Thread-safe operation on user's connection set
            lock (userLock)
            {
                var connections = _userConnections.GetOrAdd(userId, _ => new HashSet<string>());
                connections.Add(connectionId);
            }

            // Map connection back to user
            _connectionUsers.TryAdd(connectionId, userId);
            
            // Update last activity
            await UpdateUserActivityAsync(userId);

            _logger.LogInformation($"User {userId} now has connection {connectionId}");
            
            // In production, would publish event to Redis for other servers
            await Task.CompletedTask;
        }

        /// <summary>
        /// Removes a connection and returns whether user still has other connections
        /// Critical for determining when user goes offline
        /// </summary>
        public async Task<bool> RemoveConnectionAsync(string userId, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(connectionId))
                return false;

            _logger.LogDebug($"Removing connection {connectionId} for user {userId}");

            bool hasMoreConnections = false;
            var userLock = _userLocks.GetOrAdd(userId, _ => new object());
            HashSet<string> connections = null;

            // Thread-safe removal from user's connection set
            lock (userLock)
            {
                if (_userConnections.TryGetValue(userId, out connections))
                {
                    connections.Remove(connectionId);
                    hasMoreConnections = connections.Count > 0;

                    // Clean up empty sets to prevent memory leak
                    if (!hasMoreConnections)
                    {
                        _userConnections.TryRemove(userId, out _);
                        _userLocks.TryRemove(userId, out _);
                    }
                }
            }

            // Remove reverse mapping
            _connectionUsers.TryRemove(connectionId, out _);
            
            // Remove connection metadata
            _connectionMetadata.TryRemove(connectionId, out _);

            // If no more connections, remove activity tracking
            if (!hasMoreConnections)
            {
                _userLastActivity.TryRemove(userId, out _);
                _logger.LogInformation($"User {userId} has no more connections - now offline");
            }
            else
            {
                _logger.LogInformation($"User {userId} still has {connections.Count} connections");
            }

            await Task.CompletedTask;
            return hasMoreConnections;
        }

        /// <summary>
        /// Gets all active connections for a user
        /// Used for routing messages to all user's devices
        /// </summary>
        public async Task<IEnumerable<string>> GetConnectionsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Enumerable.Empty<string>();

            if (_userConnections.TryGetValue(userId, out var connections))
            {
                // Return a copy to prevent external modification
                lock (_userLocks.GetOrAdd(userId, _ => new object()))
                {
                    return connections.ToList();
                }
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Force logout - removes all connections for a user
        /// </summary>
        public async Task<int> RemoveAllConnectionsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            _logger.LogWarning($"Removing all connections for user {userId}");

            int removedCount = 0;

            if (_userConnections.TryRemove(userId, out var connections))
            {
                var userLock = _userLocks.GetOrAdd(userId, _ => new object());
                
                lock (userLock)
                {
                    removedCount = connections.Count;
                    
                    // Remove reverse mappings
                    foreach (var connectionId in connections)
                    {
                        _connectionUsers.TryRemove(connectionId, out _);
                        _connectionMetadata.TryRemove(connectionId, out _);
                    }
                }
                
                _userLocks.TryRemove(userId, out _);
            }

            // Remove activity tracking
            _userLastActivity.TryRemove(userId, out _);

            _logger.LogWarning($"Removed {removedCount} connections for user {userId}");
            
            await Task.CompletedTask;
            return removedCount;
        }

        #endregion

        #region User Lookup

        /// <summary>
        /// Reverse lookup - finds user by connection ID
        /// Essential for handling disconnect events
        /// </summary>
        public async Task<string> GetUserIdByConnectionAsync(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                return null;

            _connectionUsers.TryGetValue(connectionId, out var userId);
            
            await Task.CompletedTask;
            return userId;
        }

        #endregion

        #region Presence Tracking

        /// <summary>
        /// Quick check if user has any active connections
        /// </summary>
        public async Task<bool> IsUserOnlineAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var isOnline = _userConnections.ContainsKey(userId);
            
            await Task.CompletedTask;
            return isOnline;
        }

        /// <summary>
        /// Gets all currently online users
        /// In production, consider pagination or filtering
        /// </summary>
        public async Task<IEnumerable<string>> GetOnlineUsersAsync()
        {
            // Return copy of keys to prevent modification during enumeration
            var onlineUsers = _userConnections.Keys.ToList();
            
            _logger.LogDebug($"Currently {onlineUsers.Count} users online");
            
            await Task.CompletedTask;
            return onlineUsers;
        }

        /// <summary>
        /// Updates user's last activity timestamp
        /// Can be used for "away" status detection
        /// </summary>
        public async Task UpdateUserActivityAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            _userLastActivity.AddOrUpdate(userId, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets users who have been inactive for specified duration
        /// Useful for setting "away" status automatically
        /// </summary>
        public async Task<IEnumerable<string>> GetInactiveUsersAsync(TimeSpan inactivityThreshold)
        {
            var threshold = DateTime.UtcNow - inactivityThreshold;
            
            var inactiveUsers = _userLastActivity
                .Where(kvp => kvp.Value < threshold)
                .Select(kvp => kvp.Key)
                .ToList();

            _logger.LogDebug($"Found {inactiveUsers.Count} inactive users");
            
            await Task.CompletedTask;
            return inactiveUsers;
        }

        #endregion

        #region Connection Metadata

        /// <summary>
        /// Stores metadata about a connection
        /// Enables rich presence features (device type, location, etc.)
        /// </summary>
        public async Task SetConnectionMetadataAsync(string connectionId, Dictionary<string, string> metadata)
        {
            if (string.IsNullOrWhiteSpace(connectionId) || metadata == null)
                return;

            _connectionMetadata.AddOrUpdate(connectionId, metadata, (_, _) => metadata);
            
            _logger.LogDebug($"Set metadata for connection {connectionId}: {metadata.Count} items");
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves metadata for a connection
        /// </summary>
        public async Task<Dictionary<string, string>> GetConnectionMetadataAsync(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                return new Dictionary<string, string>();

            if (_connectionMetadata.TryGetValue(connectionId, out var metadata))
            {
                // Return copy to prevent external modification
                return new Dictionary<string, string>(metadata);
            }

            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets all connections with metadata for a user
        /// Useful for showing user's active sessions
        /// </summary>
        public async Task<Dictionary<string, Dictionary<string, string>>> GetUserConnectionsWithMetadataAsync(string userId)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();

            var connections = await GetConnectionsAsync(userId);
            
            foreach (var connectionId in connections)
            {
                var metadata = await GetConnectionMetadataAsync(connectionId);
                result[connectionId] = metadata;
            }

            return result;
        }

        #endregion

        #region Cleanup and Maintenance

        /// <summary>
        /// Removes stale connections based on last activity
        /// Would typically run as a background service
        /// </summary>
        public async Task<int> CleanupStaleConnectionsAsync(TimeSpan staleThreshold)
        {
            var threshold = DateTime.UtcNow - staleThreshold;
            var staleUsers = new List<string>();
            
            // Find users with stale activity
            foreach (var kvp in _userLastActivity)
            {
                if (kvp.Value < threshold)
                {
                    staleUsers.Add(kvp.Key);
                }
            }

            var totalRemoved = 0;
            
            // Remove all connections for stale users
            foreach (var userId in staleUsers)
            {
                var removed = await RemoveAllConnectionsAsync(userId);
                totalRemoved += removed;
            }

            if (totalRemoved > 0)
            {
                _logger.LogWarning($"Cleaned up {totalRemoved} stale connections from {staleUsers.Count} users");
            }

            return totalRemoved;
        }

        #endregion
    }
}