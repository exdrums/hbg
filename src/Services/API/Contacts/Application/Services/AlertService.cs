using API.Contacts.Application.Dtos;
using API.Contacts.Application.Interfaces;
using API.Contacts.Domain.Models;
using API.Contacts.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Application.Services
{
    /// <summary>
    /// Implementation of the alert service for managing user alerts and notifications
    /// </summary>
    public class AlertService : IAlertService
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRealtimeNotificationService _notificationService;
        private readonly ILogger<AlertService> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public AlertService(
            IAlertRepository alertRepository,
            IUserRepository userRepository,
            IRealtimeNotificationService notificationService,
            ILogger<AlertService> logger)
        {
            _alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets active alerts for a user
        /// </summary>
        public async Task<IEnumerable<AlertDto>> GetActiveAlertsForUserAsync(string userId, int limit = 50)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                var alerts = await _alertRepository.GetActiveForUserAsync(userId, limit);
                return alerts.Select(MapToAlertDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active alerts for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets alerts for a user related to a specific conversation
        /// </summary>
        public async Task<IEnumerable<AlertDto>> GetAlertsForConversationAsync(string userId, string conversationId, int limit = 20)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            try
            {
                var alerts = await _alertRepository.GetForConversationAsync(userId, conversationId, limit);
                return alerts.Select(MapToAlertDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for conversation {ConversationId} for user {UserId}",
                    conversationId, userId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new alert
        /// </summary>
        public async Task<AlertDto> CreateAlertAsync(string userId, AlertType type, string text, string conversationId = null, string messageId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Alert text cannot be empty", nameof(text));
            }

            try
            {
                // Verify the user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User {userId} not found");
                }

                // Create the alert
                var alertId = Guid.NewGuid().ToString();
                var alert = new Alert(alertId, userId, type, text, conversationId, messageId);

                await _alertRepository.AddAsync(alert);

                _logger.LogInformation("Created {AlertType} alert {AlertId} for user {UserId}",
                    type, alertId, userId);

                // Get updated alerts and notify the user
                var activeAlerts = await _alertRepository.GetActiveForUserAsync(userId);
                var alertDtos = activeAlerts.Select(MapToAlertDto).ToList();

                await _notificationService.NotifyAlertsChanged(userId, alertDtos);

                return MapToAlertDto(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new message alert
        /// </summary>
        public async Task<AlertDto> CreateMessageAlertAsync(string userId, string conversationId, string messageId, string authorName)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentNullException(nameof(messageId));
            }

            if (string.IsNullOrEmpty(authorName))
            {
                throw new ArgumentNullException(nameof(authorName));
            }

            try
            {
                // Create a message alert
                var alertId = Guid.NewGuid().ToString();
                var text = $"New message from {authorName}";
                var alert = new Alert(alertId, userId, AlertType.NewMessage, text, conversationId, messageId);

                await _alertRepository.AddAsync(alert);

                _logger.LogInformation("Created message alert {AlertId} for user {UserId} from {AuthorName}",
                    alertId, userId, authorName);

                // Get updated alerts and notify the user
                var activeAlerts = await _alertRepository.GetActiveForUserAsync(userId);
                var alertDtos = activeAlerts.Select(MapToAlertDto).ToList();

                await _notificationService.NotifyAlertsChanged(userId, alertDtos);

                return MapToAlertDto(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating message alert for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new system alert
        /// </summary>
        public async Task<AlertDto> CreateSystemAlertAsync(string userId, string text)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Alert text cannot be empty", nameof(text));
            }

            try
            {
                // Create a system alert
                var alertId = Guid.NewGuid().ToString();
                var alert = new Alert(alertId, userId, AlertType.System, text);

                await _alertRepository.AddAsync(alert);

                _logger.LogInformation("Created system alert {AlertId} for user {UserId}", alertId, userId);

                // Get updated alerts and notify the user
                var activeAlerts = await _alertRepository.GetActiveForUserAsync(userId);
                var alertDtos = activeAlerts.Select(MapToAlertDto).ToList();

                await _notificationService.NotifyAlertsChanged(userId, alertDtos);

                return MapToAlertDto(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating system alert for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Marks an alert as read
        /// </summary>
        public async Task MarkAsReadAsync(string alertId, string userId)
        {
            if (string.IsNullOrEmpty(alertId))
            {
                throw new ArgumentNullException(nameof(alertId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Verify the alert exists and belongs to the user
                var alert = await _alertRepository.GetByIdAsync(alertId);
                if (alert == null)
                {
                    throw new KeyNotFoundException($"Alert {alertId} not found");
                }

                if (alert.UserId != userId)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to access alert {alertId}");
                }

                // Mark the alert as read
                await _alertRepository.MarkAsReadAsync(alertId);

                _logger.LogInformation("Marked alert {AlertId} as read for user {UserId}", alertId, userId);

                // Get updated alerts and notify the user
                var activeAlerts = await _alertRepository.GetActiveForUserAsync(userId);
                var alertDtos = activeAlerts.Select(MapToAlertDto).ToList();

                await _notificationService.NotifyAlertsChanged(userId, alertDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking alert {AlertId} as read for user {UserId}", alertId, userId);
                throw;
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

            try
            {
                // Mark all alerts as read
                var count = await _alertRepository.MarkAllAsReadAsync(userId);

                _logger.LogInformation("Marked {Count} alerts as read for user {UserId}", count, userId);

                // Get updated alerts and notify the user
                var activeAlerts = await _alertRepository.GetActiveForUserAsync(userId);
                var alertDtos = activeAlerts.Select(MapToAlertDto).ToList();

                await _notificationService.NotifyAlertsChanged(userId, alertDtos);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all alerts as read for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Dismisses an alert
        /// </summary>
        public async Task DismissAlertAsync(string alertId, string userId)
        {
            if (string.IsNullOrEmpty(alertId))
            {
                throw new ArgumentNullException(nameof(alertId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Verify the alert exists and belongs to the user
                var alert = await _alertRepository.GetByIdAsync(alertId);
                if (alert == null)
                {
                    throw new KeyNotFoundException($"Alert {alertId} not found");
                }

                if (alert.UserId != userId)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to access alert {alertId}");
                }

                // Dismiss the alert
                await _alertRepository.DismissAsync(alertId);

                _logger.LogInformation("Dismissed alert {AlertId} for user {UserId}", alertId, userId);

                // Get updated alerts and notify the user
                var activeAlerts = await _alertRepository.GetActiveForUserAsync(userId);
                var alertDtos = activeAlerts.Select(MapToAlertDto).ToList();

                await _notificationService.NotifyAlertsChanged(userId, alertDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing alert {AlertId} for user {UserId}", alertId, userId);
                throw;
            }
        }

        /// <summary>
        /// Dismisses all alerts for a user
        /// </summary>
        public async Task<int> DismissAllAlertsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Dismiss all alerts
                var count = await _alertRepository.DismissAllAsync(userId);

                _logger.LogInformation("Dismissed {Count} alerts for user {UserId}", count, userId);

                // Get updated alerts and notify the user
                var activeAlerts = await _alertRepository.GetActiveForUserAsync(userId);
                var alertDtos = activeAlerts.Select(MapToAlertDto).ToList();

                await _notificationService.NotifyAlertsChanged(userId, alertDtos);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing all alerts for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Dismisses alerts for a specific conversation
        /// </summary>
        public async Task<int> DismissAlertsForConversationAsync(string userId, string conversationId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            try
            {
                // Dismiss alerts for the conversation
                var count = await _alertRepository.DismissForConversationAsync(userId, conversationId);

                _logger.LogInformation("Dismissed {Count} alerts for conversation {ConversationId} for user {UserId}",
                    count, conversationId, userId);

                // Get updated alerts and notify the user
                var activeAlerts = await _alertRepository.GetActiveForUserAsync(userId);
                var alertDtos = activeAlerts.Select(MapToAlertDto).ToList();

                await _notificationService.NotifyAlertsChanged(userId, alertDtos);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing alerts for conversation {ConversationId} for user {UserId}",
                    conversationId, userId);
                throw;
            }
        }

        /// <summary>
        /// Gets the count of unread alerts for a user
        /// </summary>
        public async Task<int> GetUnreadAlertCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                return await _alertRepository.GetUnreadCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread alert count for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Maps an Alert domain entity to an AlertDto
        /// </summary>
        private AlertDto MapToAlertDto(Alert alert)
        {
            if (alert == null)
            {
                return null;
            }

            return new AlertDto
            {
                Id = alert.Id,
                Type = alert.Type,
                Text = alert.Text,
                CreatedAt = alert.CreatedAt,
                IsRead = alert.IsRead,
                ConversationId = alert.ConversationId,
                MessageId = alert.MessageId
            };
        }
    }
}
