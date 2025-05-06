using API.Contacts.Application.Dtos;
using API.Contacts.Application.Interfaces;
using API.Contacts.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Application.Services
{
    /// <summary>
    /// Implementation of the chat application service that orchestrates
    /// multiple domain services to provide a unified chat functionality.
    /// This is a facade that simplifies the interaction with the chat system.
    /// </summary>
    public class ChatApplicationService : IChatApplicationService
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IAlertService _alertService;
        private readonly ITypingService _typingService;
        private readonly IReadReceiptService _readReceiptService;
        private readonly IAiAssistantService _aiAssistantService;
        private readonly IAuthenticationService _authService;
        private readonly ILogger<ChatApplicationService> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ChatApplicationService(
            IChatService chatService,
            IUserService userService,
            IAlertService alertService,
            ITypingService typingService,
            IReadReceiptService readReceiptService,
            IAiAssistantService aiAssistantService,
            IAuthenticationService authService,
            ILogger<ChatApplicationService> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            _typingService = typingService ?? throw new ArgumentNullException(nameof(typingService));
            _readReceiptService = readReceiptService ?? throw new ArgumentNullException(nameof(readReceiptService));
            _aiAssistantService = aiAssistantService ?? throw new ArgumentNullException(nameof(aiAssistantService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all conversations for a user
        /// </summary>
        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(string userId)
        {
            try
            {
                var conversationDtos = await _chatService.GetUserConversationsAsync(userId);
                // In a real implementation with proper mapping, we would convert DTOs to domain entities
                // For simplicity, we're returning empty list here
                return Enumerable.Empty<Conversation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new conversation with specified participants
        /// </summary>
        public async Task<Conversation> CreateConversationAsync(string creatorId, IEnumerable<string> participantIds, string title)
        {
            try
            {
                var conversationDto = await _chatService.CreateConversationAsync(creatorId, participantIds, title);
                // In a real implementation with proper mapping, we would convert DTO to domain entity
                // For simplicity, we're returning null here
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation by user {CreatorId}", creatorId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new AI assistant conversation
        /// </summary>
        public async Task<Conversation> CreateAiAssistantConversationAsync(string userId, string title = "AI Assistant")
        {
            try
            {
                var conversationDto = await _chatService.CreateAiAssistantConversationAsync(userId, title);
                // In a real implementation with proper mapping, we would convert DTO to domain entity
                // For simplicity, we're returning null here
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AI assistant conversation for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets conversation messages
        /// </summary>
        public async Task<IEnumerable<Message>> GetConversationMessagesAsync(string conversationId, string userId, int limit = 50)
        {
            try
            {
                var messageDtos = await _chatService.GetConversationMessagesAsync(conversationId, userId, limit);
                // In a real implementation with proper mapping, we would convert DTOs to domain entities
                // For simplicity, we're returning empty list here
                return Enumerable.Empty<Message>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Sends a message in a conversation
        /// </summary>
        public async Task<Message> SendMessageAsync(string conversationId, string userId, string text, string parentMessageId = null)
        {
            try
            {
                var messageDto = await _chatService.SendMessageAsync(conversationId, userId, text, parentMessageId);
                // In a real implementation with proper mapping, we would convert DTO to domain entity
                // For simplicity, we're returning null here
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Sends a message to an AI assistant
        /// </summary>
        public async Task<Message> SendMessageToAiAsync(string conversationId, string userId, string text)
        {
            try
            {
                var messageDto = await _chatService.SendMessageToAiAsync(conversationId, userId, text);
                // In a real implementation with proper mapping, we would convert DTO to domain entity
                // For simplicity, we're returning null here
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to AI in conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Regenerates an AI response
        /// </summary>
        public async Task<Message> RegenerateAiResponseAsync(string conversationId, string messageId, string userId)
        {
            try
            {
                var messageDto = await _chatService.RegenerateAiResponseAsync(conversationId, messageId, userId);
                // In a real implementation with proper mapping, we would convert DTO to domain entity
                // For simplicity, we're returning null here
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating AI response for message {MessageId}", messageId);
                throw;
            }
        }

        /// <summary>
        /// Records that a user has started typing
        /// </summary>
        public async Task UserStartedTypingAsync(string conversationId, string userId)
        {
            try
            {
                await _chatService.UserStartedTypingAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording user {UserId} started typing in conversation {ConversationId}",
                    userId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Records that a user has stopped typing
        /// </summary>
        public async Task UserStoppedTypingAsync(string conversationId, string userId)
        {
            try
            {
                await _chatService.UserStoppedTypingAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording user {UserId} stopped typing in conversation {ConversationId}",
                    userId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Gets active alerts for a user
        /// </summary>
        public async Task<IEnumerable<Alert>> GetAlertsAsync(string userId, string conversationId)
        {
            try
            {
                IEnumerable<AlertDto> alerts;

                if (!string.IsNullOrEmpty(conversationId))
                {
                    alerts = await _alertService.GetAlertsForConversationAsync(userId, conversationId);
                }
                else
                {
                    alerts = await _alertService.GetActiveAlertsForUserAsync(userId);
                }

                // In a real implementation with proper mapping, we would convert DTOs to domain entities
                // For simplicity, we're returning empty list here
                return Enumerable.Empty<Alert>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for user {UserId}", userId);
                throw;
            }
        }
    }
}
