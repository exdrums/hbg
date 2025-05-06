using API.Contacts.Application.Dtos;
using API.Contacts.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace API.Contacts.Infrastructure.SignalR
{
    /// <summary>
    /// SignalR Hub for chat functionality
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IAuthenticationService _authService;
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly ITypingService _typingService;
        private readonly IAlertService _alertService;
        private readonly IReadReceiptService _readReceiptService;
        private readonly IFileMessageHandler _fileMessageHandler;
        private readonly ILogger<ChatHub> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ChatHub(
            IConnectionManager connectionManager,
            IAuthenticationService authService,
            IChatService chatService,
            IUserService userService,
            ITypingService typingService,
            IAlertService alertService,
            IReadReceiptService readReceiptService,
            IFileMessageHandler fileMessageHandler,
            ILogger<ChatHub> logger)
        {
            _connectionManager = connectionManager;
            _authService = authService;
            _chatService = chatService;
            _userService = userService;
            _typingService = typingService;
            _alertService = alertService;
            _readReceiptService = readReceiptService;
            _fileMessageHandler = fileMessageHandler;
            _logger = logger;
        }

        /// <summary>
        /// Handles client connection
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                // Get the user ID from the connection context
                var oidcSubject = Context.User?.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(oidcSubject))
                {
                    // Get or create user based on OIDC information
                    var displayName = Context.User?.FindFirst("name")?.Value ?? "User";
                    var user = await _userService.GetOrCreateUserFromOidcAsync(oidcSubject, displayName);

                    // Store connection information
                    _connectionManager.AddConnection(Context.ConnectionId, user.Id);

                    // Load user's active alerts
                    var alerts = await _alertService.GetActiveAlertsForUserAsync(user.Id);
                    await Clients.Caller.SendAsync("AlertsChanged", alerts);

                    _logger.LogInformation("User {UserId} connected with connection ID {ConnectionId}", user.Id, Context.ConnectionId);
                }
                else
                {
                    _logger.LogWarning("Unauthenticated connection attempt rejected");
                    throw new HubException("User is not authenticated");
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
                throw;
            }
        }

        /// <summary>
        /// Handles client disconnection
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = _connectionManager.GetUserIdForConnection(Context.ConnectionId);
                _connectionManager.RemoveConnection(Context.ConnectionId);

                if (!string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation("User {UserId} disconnected from connection ID {ConnectionId}", userId, Context.ConnectionId);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
        }

        /// <summary>
        /// Join a specific conversation channel
        /// </summary>
        public async Task JoinConversation(string conversationId)
        {
            try
            {
                var userId = await GetUserIdFromContext();

                // Check if the user is authorized to join this conversation
                var isAuthorized = await _authService.IsUserAuthorizedForConversationAsync(userId, conversationId);

                if (!isAuthorized)
                {
                    _logger.LogWarning("User {UserId} attempted to join unauthorized conversation {ConversationId}", userId, conversationId);
                    throw new HubException("User is not authorized to join this conversation");
                }

                _connectionManager.AddToConversation(Context.ConnectionId, conversationId);
                _logger.LogInformation("User {UserId} joined conversation {ConversationId}", userId, conversationId);

                // Load previous messages
                var messages = await _chatService.GetConversationMessagesAsync(conversationId, userId, 50);
                await Clients.Caller.SendAsync("LoadMessages", conversationId, messages);

                // Load typing users
                var typingUsers = await _typingService.GetUsersTypingAsync(conversationId, userId);
                if (typingUsers.Any())
                {
                    await Clients.Caller.SendAsync("UsersTyping", conversationId, typingUsers);
                }

                // Load read receipts
                var readReceipts = await _readReceiptService.GetReadReceiptsForConversationAsync(conversationId);
                await Clients.Caller.SendAsync("ReadReceiptsUpdated", conversationId, readReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining conversation {ConversationId}", conversationId);
                throw new HubException("Failed to join conversation: " + ex.Message);
            }
        }

        /// <summary>
        /// Leave a conversation channel
        /// </summary>
        public void LeaveConversation(string conversationId)
        {
            try
            {
                _connectionManager.RemoveFromConversation(Context.ConnectionId, conversationId);
                _logger.LogInformation("Connection {ConnectionId} left conversation {ConversationId}", Context.ConnectionId, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving conversation {ConversationId}", conversationId);
                throw new HubException("Failed to leave conversation: " + ex.Message);
            }
        }

        /// <summary>
        /// Load all conversations for the current user
        /// </summary>
        public async Task<IEnumerable<ConversationDto>> LoadConversations()
        {
            try
            {
                var userId = await GetUserIdFromContext();
                return await _chatService.GetUserConversationsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading conversations");
                throw new HubException("Failed to load conversations: " + ex.Message);
            }
        }

        /// <summary>
        /// Send a message to a conversation
        /// </summary>
        public async Task<MessageDto> SendMessage(string conversationId, string text, string parentMessageId = null)
        {
            try
            {
                var userId = await GetUserIdFromContext();
                return await _chatService.SendMessageAsync(conversationId, userId, text, parentMessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to conversation {ConversationId}", conversationId);
                throw new HubException("Failed to send message: " + ex.Message);
            }
        }

        /// <summary>
        /// Send a message to an AI assistant conversation
        /// </summary>
        public async Task<MessageDto> SendMessageToAi(string conversationId, string text)
        {
            try
            {
                var userId = await GetUserIdFromContext();
                return await _chatService.SendMessageToAiAsync(conversationId, userId, text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to AI in conversation {ConversationId}", conversationId);
                throw new HubException("Failed to send message to AI: " + ex.Message);
            }
        }

        /// <summary>
        /// Create a new conversation with specified participants
        /// </summary>
        public async Task<ConversationDto> CreateConversation(IEnumerable<string> participantIds, string title = null)
        {
            try
            {
                var userId = await GetUserIdFromContext();
                return await _chatService.CreateConversationAsync(userId, participantIds, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                throw new HubException("Failed to create conversation: " + ex.Message);
            }
        }

        /// <summary>
        /// Create a new AI assistant conversation
        /// </summary>
        public async Task<ConversationDto> CreateAiAssistantConversation(string title = "AI Assistant")
        {
            try
            {
                var userId = await GetUserIdFromContext();
                return await _chatService.CreateAiAssistantConversationAsync(userId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AI assistant conversation");
                throw new HubException("Failed to create AI assistant conversation: " + ex.Message);
            }
        }

        /// <summary>
        /// Regenerate an AI assistant response
        /// </summary>
        public async Task<MessageDto> RegenerateAiResponse(string conversationId, string messageId)
        {
            try
            {
                var userId = await GetUserIdFromContext();
                return await _chatService.RegenerateAiResponseAsync(conversationId, messageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating AI response for message {MessageId}", messageId);
                throw new HubException("Failed to regenerate AI response: " + ex.Message);
            }
        }

        /// <summary>
        /// Indicate that the user has started typing
        /// </summary>
        public async Task UserStartedTyping(string conversationId)
        {
            try
            {
                var userId = await GetUserIdFromContext();
                await _typingService.UserStartedTypingAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserStartedTyping for conversation {ConversationId}", conversationId);
                // Don't throw an exception for typing indicators to avoid disrupting the user experience
            }
        }

        /// <summary>
        /// Indicate that the user has stopped typing
        /// </summary>
        public async Task UserStoppedTyping(string conversationId)
        {
            try
            {
                var userId = await GetUserIdFromContext();
                await _typingService.UserStoppedTypingAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserStoppedTyping for conversation {ConversationId}", conversationId);
                // Don't throw an exception for typing indicators to avoid disrupting the user experience
            }
        }

        /// <summary>
        /// Mark messages as read up to the current timestamp
        /// </summary>
        public async Task MarkAsRead(string conversationId)
        {
            try
            {
                var userId = await GetUserIdFromContext();
                _logger.LogInformation("User {UserId} marking messages as read in conversation {ConversationId}", userId, conversationId);

                await _chatService.MarkConversationAsReadAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read in conversation {ConversationId}", conversationId);
                throw new HubException("Failed to mark messages as read: " + ex.Message);
            }
        }

        /// <summary>
        /// Get read receipts for a conversation
        /// </summary>
        public async Task<IDictionary<string, DateTime>> GetReadReceipts(string conversationId)
        {
            try
            {
                var userId = await GetUserIdFromContext();
                return await _readReceiptService.GetReadReceiptsForConversationAsync(conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting read receipts for conversation {ConversationId}", conversationId);
                throw new HubException("Failed to get read receipts: " + ex.Message);
            }
        }

        /// <summary>
        /// Check if a message contains a file attachment
        /// </summary>
        public bool IsFileMessage(string messageText)
        {
            return _fileMessageHandler.IsFileMessage(messageText);
        }

        /// <summary>
        /// Extract file information from a message
        /// </summary>
        public (string FileName, string FileUrl)? GetFileInfo(string messageText)
        {
            return _fileMessageHandler.ExtractFileInfo(messageText);
        }

        /// <summary>
        /// Helper method to get the current user ID from context
        /// </summary>
        private async Task<string> GetUserIdFromContext()
        {
            var oidcSubject = Context.User?.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(oidcSubject))
            {
                throw new HubException("User is not authenticated");
            }

            var user = await _userService.GetOrCreateUserFromOidcAsync(oidcSubject, null);
            return user.Id;
        }
    }
}
