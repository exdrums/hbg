using API.Contacts.Application.Dtos;
using API.Contacts.Application.Interfaces;
using API.Contacts.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace API.Contacts.Application.Services
{
    /// <summary>
    /// Implementation of the typing service for handling typing indicators
    /// </summary>
    public class TypingService : ITypingService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRealtimeNotificationService _notificationService;
        private readonly ILogger<TypingService> _logger;

        // Concurrent dictionary to store typing status:
        // conversationId -> userId -> CancellationTokenSource
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, CancellationTokenSource>> _typingUsers = new();

        // Typing indicator timeout (in seconds)
        private const int TypingTimeoutSeconds = 5;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public TypingService(
            IUserRepository userRepository,
            IRealtimeNotificationService notificationService,
            ILogger<TypingService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Records that a user has started typing in a conversation
        /// </summary>
        public async Task UserStartedTypingAsync(string conversationId, string userId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Get the user to make sure they exist
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User {userId} not found");
                }

                // Get or create the conversation dictionary
                var conversationDict = _typingUsers.GetOrAdd(conversationId, _ => new ConcurrentDictionary<string, CancellationTokenSource>());

                // Cancel any existing typing timeout
                if (conversationDict.TryGetValue(userId, out var existingCts))
                {
                    existingCts.Cancel();
                }

                // Create a new cancellation token source for the typing timeout
                var cts = new CancellationTokenSource();
                conversationDict[userId] = cts;

                // Schedule the typing timeout
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(TypingTimeoutSeconds), cts.Token);

                        // If the delay completes without being canceled, the user has stopped typing
                        await UserStoppedTypingAsync(conversationId, userId);
                    }
                    catch (TaskCanceledException)
                    {
                        // This is expected when the typing is refreshed or manually stopped
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in typing timeout for user {UserId} in conversation {ConversationId}",
                            userId, conversationId);
                    }
                });

                // Check if this is a new typing notification or a refresh
                bool isNewTyping = !conversationDict.ContainsKey(userId);

                // If it's a new typing notification, notify other users
                if (isNewTyping)
                {
                    var userDto = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        AvatarUrl = user.AvatarUrl,
                        AvatarAlt = user.AvatarAlt
                    };

                    await _notificationService.NotifyUserStartedTyping(conversationId, userDto);

                    _logger.LogDebug("User {UserId} started typing in conversation {ConversationId}", userId, conversationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording typing started for user {UserId} in conversation {ConversationId}",
                    userId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Records that a user has stopped typing in a conversation
        /// </summary>
        public async Task UserStoppedTypingAsync(string conversationId, string userId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Check if the conversation exists in our tracking
                if (!_typingUsers.TryGetValue(conversationId, out var conversationDict))
                {
                    return; // Nothing to do
                }

                // Remove the user from the typing list and cancel any timeout
                if (conversationDict.TryRemove(userId, out var cts))
                {
                    cts.Cancel();

                    // Get the user to include in the notification
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        var userDto = new UserDto
                        {
                            Id = user.Id,
                            Name = user.Name,
                            AvatarUrl = user.AvatarUrl,
                            AvatarAlt = user.AvatarAlt
                        };

                        await _notificationService.NotifyUserStoppedTyping(conversationId, userDto);

                        _logger.LogDebug("User {UserId} stopped typing in conversation {ConversationId}", userId, conversationId);
                    }

                    // If this was the last typing user, remove the conversation entry
                    if (conversationDict.IsEmpty)
                    {
                        _typingUsers.TryRemove(conversationId, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording typing stopped for user {UserId} in conversation {ConversationId}",
                    userId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Gets users who are currently typing in a conversation
        /// </summary>
        public async Task<IEnumerable<UserDto>> GetUsersTypingAsync(string conversationId, string excludeUserId = null)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            try
            {
                var typingUserDtos = new List<UserDto>();

                // Check if the conversation exists in our tracking
                if (_typingUsers.TryGetValue(conversationId, out var conversationDict))
                {
                    // Get all user IDs who are typing
                    var typingUserIds = conversationDict.Keys.ToList();

                    // Exclude the specified user ID if provided
                    if (!string.IsNullOrEmpty(excludeUserId))
                    {
                        typingUserIds.Remove(excludeUserId);
                    }

                    // Get user information for each typing user
                    foreach (var userId in typingUserIds)
                    {
                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user != null)
                        {
                            typingUserDtos.Add(new UserDto
                            {
                                Id = user.Id,
                                Name = user.Name,
                                AvatarUrl = user.AvatarUrl,
                                AvatarAlt = user.AvatarAlt
                            });
                        }
                    }
                }

                return typingUserDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting typing users for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Checks if a user is currently typing in a conversation
        /// </summary>
        public async Task<bool> IsUserTypingAsync(string conversationId, string userId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Check if the conversation exists in our tracking
                if (!_typingUsers.TryGetValue(conversationId, out var conversationDict))
                {
                    return false;
                }

                // Check if the user is in the typing list
                return conversationDict.ContainsKey(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is typing in conversation {ConversationId}",
                    userId, conversationId);
                throw;
            }
        }
    }
}
