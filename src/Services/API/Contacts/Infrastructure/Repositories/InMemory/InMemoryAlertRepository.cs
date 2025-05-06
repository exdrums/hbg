using API.Contacts.Domain.Models;
using API.Contacts.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Infrastructure.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of the alert repository
    /// </summary>
    public class InMemoryAlertRepository : InMemoryRepositoryBase<Alert>, IAlertRepository
    {
        private readonly ILogger<InMemoryAlertRepository> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public InMemoryAlertRepository(ILogger<InMemoryAlertRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the ID for an alert
        /// </summary>
        protected override string GetId(Alert alert)
        {
            return alert?.Id;
        }

        /// <summary>
        /// Gets active alerts for a user
        /// </summary>
        public async Task<IEnumerable<Alert>> GetActiveForUserAsync(string userId, int limit = 50)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                var activeAlerts = _entities.Values
                    .Where(a => a.UserId == userId)
                    .Where(a => !a.IsDismissed)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(limit)
                    .ToList();

                return Task.FromResult(activeAlerts.AsEnumerable());
            }
        }

        /// <summary>
        /// Gets alerts for a user related to a specific conversation
        /// </summary>
        public async Task<IEnumerable<Alert>> GetForConversationAsync(string userId, string conversationId, int limit = 20)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            lock (_lock)
            {
                var conversationAlerts = _entities.Values
                    .Where(a => a.UserId == userId)
                    .Where(a => a.ConversationId == conversationId)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(limit)
                    .ToList();

                return Task.FromResult(conversationAlerts.AsEnumerable());
            }
        }

        /// <summary>
        /// Marks an alert as read
        /// </summary>
        public async Task<bool> MarkAsReadAsync(string alertId)
        {
            if (string.IsNullOrEmpty(alertId))
            {
                throw new ArgumentNullException(nameof(alertId));
            }

            lock (_lock)
            {
                if (!_entities.TryGetValue(alertId, out var alert))
                {
                    return Task.FromResult(false);
                }

                alert.MarkAsRead();
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Marks all alerts as read for a user
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                var unreadAlerts = _entities.Values
                    .Where(a => a.UserId == userId)
                    .Where(a => !a.IsRead)
                    .Where(a => !a.IsDismissed)
                    .ToList();

                foreach (var alert in unreadAlerts)
                {
                    alert.MarkAsRead();
                }

                return Task.FromResult(unreadAlerts.Count);
            }
        }

        /// <summary>
        /// Dismisses an alert
        /// </summary>
        public async Task<bool> DismissAsync(string alertId)
        {
            if (string.IsNullOrEmpty(alertId))
            {
                throw new ArgumentNullException(nameof(alertId));
            }

            lock (_lock)
            {
                if (!_entities.TryGetValue(alertId, out var alert))
                {
                    return Task.FromResult(false);
                }

                alert.Dismiss();
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Dismisses all alerts for a user
        /// </summary>
        public async Task<int> DismissAllAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                var activeAlerts = _entities.Values
                    .Where(a => a.UserId == userId)
                    .Where(a => !a.IsDismissed)
                    .ToList();

                foreach (var alert in activeAlerts)
                {
                    alert.Dismiss();
                }

                return Task.FromResult(activeAlerts.Count);
            }
        }

        /// <summary>
        /// Dismisses alerts for a specific conversation
        /// </summary>
        public async Task<int> DismissForConversationAsync(string userId, string conversationId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            lock (_lock)
            {
                var conversationAlerts = _entities.Values
                    .Where(a => a.UserId == userId)
                    .Where(a => a.ConversationId == conversationId)
                    .Where(a => !a.IsDismissed)
                    .ToList();

                foreach (var alert in conversationAlerts)
                {
                    alert.Dismiss();
                }

                return Task.FromResult(conversationAlerts.Count);
            }
        }

        /// <summary>
        /// Gets the count of unread alerts for a user
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                var unreadCount = _entities.Values
                    .Count(a => a.UserId == userId && !a.IsRead && !a.IsDismissed);

                return Task.FromResult(unreadCount);
            }
        }
    }
}
