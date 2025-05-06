using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Contacts.Application.Dtos;
using API.Contacts.Application.Interfaces;
using API.Contacts.Domain.Models;
using API.Contacts.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace API.Contacts.Application.Services
{
    /// <summary>
    /// Implementation of the chat service that coordinates chat operations
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authService;
        private readonly IRealtimeNotificationService _notificationService;
        private readonly IReadReceiptService _readReceiptService;
        private readonly ITypingService _typingService;
        private readonly IAiAssistantService _aiAssistantService;
        private readonly ILogger<ChatService> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ChatService(
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IUserRepository userRepository,
            IAuthenticationService authService,
            IRealtimeNotificationService notificationService,
            IReadReceiptService readReceiptService,
            ITypingService typingService,
            IAiAssistantService aiAssistantService,
            ILogger<ChatService> logger)
        {
            _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _readReceiptService = readReceiptService ?? throw new ArgumentNullException(nameof(readReceiptService));
            _typingService = typingService ?? throw new ArgumentNullException(nameof(typingService));
            _aiAssistantService = aiAssistantService ?? throw new ArgumentNullException(nameof(aiAssistantService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all conversations for a user
        /// </summary>
        public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                var conversations = await _conversationRepository.GetByUserIdAsync(userId);
                var conversationDtos = new List<ConversationDto>();

                foreach (var conversation in conversations)
                {
                    var lastReadTimestamp = await _readReceiptService.GetLastReadTimestampAsync(userId, conversation.Id);
                    var unreadCount = await _messageRepository.GetUnreadCountAsync(conversation.Id, userId, lastReadTimestamp);

                    // Get the last message if available
                    var lastMessages = await _messageRepository.GetByConversationIdAsync(conversation.Id, 1);
                    var lastMessage = lastMessages.FirstOrDefault();
                    MessageDto lastMessageDto = null;

                    if (lastMessage != null)
                    {
                        var author = await _userRepository.GetByIdAsync(lastMessage.AuthorId);
                        lastMessageDto = new MessageDto
                        {
                            Id = lastMessage.Id,
                            Text = lastMessage.Text,
                            Timestamp = lastMessage.Timestamp,
                            Author = MapToUserDto(author),
                            IsEdited = lastMessage.IsEdited,
                            IsSystemAlert = lastMessage.IsSystemAlert,
                            ParentMessageId = lastMessage.ParentMessageId
                        };
                    }

                    conversationDtos.Add(new ConversationDto
                    {
                        Id = conversation.Id,
                        Title = conversation.Title,
                        Type = conversation.Type,
                        Participants = await GetParticipantDtosAsync(conversation),
                        LastMessageAt = conversation.LastMessageAt,
                        IsArchived = conversation.IsArchived,
                        UnreadCount = unreadCount,
                        LastMessage = lastMessageDto
                    });
                }

                return conversationDtos.OrderByDescending(c => c.LastMessageAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets a conversation by ID
        /// </summary>
        public async Task<ConversationDto> GetConversationByIdAsync(string conversationId, string userId)
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
                // Check authorization
                var isAuthorized = await _authService.IsUserAuthorizedForConversationAsync(userId, conversationId);
                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to access conversation {conversationId}");
                }

                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation {conversationId} not found");
                }

                // Get unread count
                var lastReadTimestamp = await _readReceiptService.GetLastReadTimestampAsync(userId, conversationId);
                var unreadCount = await _messageRepository.GetUnreadCountAsync(conversationId, userId, lastReadTimestamp);

                // Get last message
                var lastMessages = await _messageRepository.GetByConversationIdAsync(conversationId, 1);
                var lastMessage = lastMessages.FirstOrDefault();
                MessageDto lastMessageDto = null;

                if (lastMessage != null)
                {
                    var author = await _userRepository.GetByIdAsync(lastMessage.AuthorId);
                    lastMessageDto = new MessageDto
                    {
                        Id = lastMessage.Id,
                        Text = lastMessage.Text,
                        Timestamp = lastMessage.Timestamp,
                        Author = MapToUserDto(author),
                        IsEdited = lastMessage.IsEdited,
                        IsSystemAlert = lastMessage.IsSystemAlert,
                        ParentMessageId = lastMessage.ParentMessageId
                    };
                }

                return new ConversationDto
                {
                    Id = conversation.Id,
                    Title = conversation.Title,
                    Type = conversation.Type,
                    Participants = await GetParticipantDtosAsync(conversation),
                    LastMessageAt = conversation.LastMessageAt,
                    IsArchived = conversation.IsArchived,
                    UnreadCount = unreadCount,
                    LastMessage = lastMessageDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation {ConversationId} for user {UserId}", conversationId, userId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new conversation with the specified participants
        /// </summary>
        public async Task<ConversationDto> CreateConversationAsync(string creatorId, IEnumerable<string> participantIds, string title = null)
        {
            if (string.IsNullOrEmpty(creatorId))
            {
                throw new ArgumentNullException(nameof(creatorId));
            }

            if (participantIds == null || !participantIds.Any())
            {
                throw new ArgumentException("At least one participant is required", nameof(participantIds));
            }

            try
            {
                // Create the conversation
                var allParticipantIds = new HashSet<string>(participantIds) { creatorId };

                // If there are only two participants (creator and one other), create a one-on-one conversation
                if (allParticipantIds.Count == 2)
                {
                    var otherParticipantId = allParticipantIds.First(id => id != creatorId);

                    // Check if a one-on-one conversation already exists
                    var existingConversation = await _conversationRepository.GetOneOnOneConversationAsync(creatorId, otherParticipantId);
                    if (existingConversation != null)
                    {
                        return await GetConversationByIdAsync(existingConversation.Id, creatorId);
                    }

                    // Create a new one-on-one conversation
                    var conversation = Conversation.CreateOneOnOne(creatorId, otherParticipantId);
                    await _conversationRepository.AddAsync(conversation);

                    _logger.LogInformation("Created one-on-one conversation {ConversationId} between {CreatorId} and {OtherParticipantId}",
                        conversation.Id, creatorId, otherParticipantId);

                    return await GetConversationByIdAsync(conversation.Id, creatorId);
                }
                else
                {
                    // Create a group conversation
                    var conversation = Conversation.CreateGroup(creatorId, allParticipantIds, title);
                    await _conversationRepository.AddAsync(conversation);

                    _logger.LogInformation("Created group conversation {ConversationId} with {ParticipantCount} participants",
                        conversation.Id, allParticipantIds.Count);

                    return await GetConversationByIdAsync(conversation.Id, creatorId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation for user {CreatorId}", creatorId);
                throw;
            }
        }

        /// <summary>
        /// Creates a one-on-one conversation between two users
        /// </summary>
        public async Task<ConversationDto> CreateOneOnOneConversationAsync(string user1Id, string user2Id)
        {
            if (string.IsNullOrEmpty(user1Id))
            {
                throw new ArgumentNullException(nameof(user1Id));
            }

            if (string.IsNullOrEmpty(user2Id))
            {
                throw new ArgumentNullException(nameof(user2Id));
            }

            try
            {
                // Check if a one-on-one conversation already exists
                var existingConversation = await _conversationRepository.GetOneOnOneConversationAsync(user1Id, user2Id);
                if (existingConversation != null)
                {
                    return await GetConversationByIdAsync(existingConversation.Id, user1Id);
                }

                // Create a new one-on-one conversation
                var conversation = Conversation.CreateOneOnOne(user1Id, user2Id);
                await _conversationRepository.AddAsync(conversation);

                _logger.LogInformation("Created one-on-one conversation {ConversationId} between {User1Id} and {User2Id}",
                    conversation.Id, user1Id, user2Id);

                return await GetConversationByIdAsync(conversation.Id, user1Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating one-on-one conversation between users {User1Id} and {User2Id}", user1Id, user2Id);
                throw;
            }
        }

        /// <summary>
        /// Creates a new AI assistant conversation
        /// </summary>
        public async Task<ConversationDto> CreateAiAssistantConversationAsync(string userId, string title = "AI Assistant")
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Get or create AI assistant user
                var aiUser = await _aiAssistantService.CreateAiAssistantUserAsync();

                // Create a new AI assistant conversation
                var conversation = Conversation.CreateAiAssistant(userId, aiUser.Id, title);
                await _conversationRepository.AddAsync(conversation);

                _logger.LogInformation("Created AI assistant conversation {ConversationId} for user {UserId}", conversation.Id, userId);

                // Create a welcome message
                var systemMessageId = Guid.NewGuid().ToString();
                var welcomeMessage = new Message(
                    systemMessageId,
                    conversation.Id,
                    aiUser.Id,
                    "Hello! I'm your AI assistant. How can I help you today?",
                    DateTime.UtcNow
                );
                await _messageRepository.AddAsync(welcomeMessage);

                // Update the conversation's last message time
                conversation.UpdateLastMessageTime(welcomeMessage.Timestamp);
                await _conversationRepository.UpdateAsync(conversation);

                return await GetConversationByIdAsync(conversation.Id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AI assistant conversation for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets messages from a conversation
        /// </summary>
        public async Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(string conversationId, string userId, int limit = 50, DateTime? before = null)
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
                // Check authorization
                var isAuthorized = await _authService.IsUserAuthorizedForConversationAsync(userId, conversationId);
                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to access conversation {conversationId}");
                }

                var messages = await _messageRepository.GetByConversationIdAsync(conversationId, limit, before);
                return await MapToMessageDtosAsync(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Sends a message to a conversation
        /// </summary>
        public async Task<MessageDto> SendMessageAsync(string conversationId, string userId, string text, string parentMessageId = null)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Message text cannot be empty", nameof(text));
            }

            try
            {
                // Check authorization
                var isAuthorized = await _authService.IsUserAuthorizedForConversationAsync(userId, conversationId);
                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to send messages to conversation {conversationId}");
                }

                // If parentMessageId is provided, verify it exists
                if (!string.IsNullOrEmpty(parentMessageId))
                {
                    var parentMessage = await _messageRepository.GetByIdAsync(parentMessageId);
                    if (parentMessage == null || parentMessage.ConversationId != conversationId)
                    {
                        throw new ArgumentException($"Parent message with ID {parentMessageId} does not exist in this conversation");
                    }
                }

                // Create and save the message
                var messageId = Guid.NewGuid().ToString();
                var message = new Message(messageId, conversationId, userId, text, DateTime.UtcNow, parentMessageId);

                await _messageRepository.AddAsync(message);

                // Update the conversation's last message time
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                conversation.UpdateLastMessageTime(message.Timestamp);
                await _conversationRepository.UpdateAsync(conversation);

                // Notify that the user has stopped typing
                var user = await _userRepository.GetByIdAsync(userId);
                await _typingService.UserStoppedTypingAsync(conversationId, userId);

                // Notify about the new message
                var messageDto = await MapToMessageDtoAsync(message);
                await _notificationService.NotifyMessageReceived(conversationId, messageDto);

                _logger.LogInformation("User {UserId} sent message {MessageId} to conversation {ConversationId}",
                    userId, messageId, conversationId);

                // For AI assistant conversations, the AI response will be handled in
                // a separate method triggered by this message

                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Sends a message to an AI assistant conversation
        /// </summary>
        public async Task<MessageDto> SendMessageToAiAsync(string conversationId, string userId, string text)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Message text cannot be empty", nameof(text));
            }

            try
            {
                // Check if this is an AI conversation
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation {conversationId} not found");
                }

                if (conversation.Type != ConversationType.AiAssistant)
                {
                    throw new InvalidOperationException($"Conversation {conversationId} is not an AI assistant conversation");
                }

                // First send the user's message
                var userMessageDto = await SendMessageAsync(conversationId, userId, text);

                // Now generate and send the AI response
                try
                {
                    // Find the AI assistant participant
                    var aiParticipant = conversation.Participants.FirstOrDefault(p => p.Role == ParticipantRole.AiAssistant);
                    if (aiParticipant == null)
                    {
                        throw new InvalidOperationException($"No AI assistant found in conversation {conversationId}");
                    }

                    // Check if AI request limit is reached
                    if (await _aiAssistantService.IsRequestLimitReachedAsync(userId))
                    {
                        // Create a system message indicating the rate limit
                        var systemMessage = Message.CreateSystemAlert(
                            conversationId,
                            "Request limit reached. Please try again later."
                        );
                        await _messageRepository.AddAsync(systemMessage);

                        // Update the conversation's last message time
                        conversation.UpdateLastMessageTime(systemMessage.Timestamp);
                        await _conversationRepository.UpdateAsync(conversation);

                        // Notify about the system message
                        var systemMessageDto = await MapToMessageDtoAsync(systemMessage);
                        await _notificationService.NotifyMessageReceived(conversationId, systemMessageDto);

                        return userMessageDto;
                    }

                    // Notify that AI is "typing"
                    var aiUser = await _userRepository.GetByIdAsync(aiParticipant.UserId);
                    var aiUserDto = MapToUserDto(aiUser);
                    await _notificationService.NotifyUserStartedTyping(conversationId, aiUserDto);

                    // Get conversation history for context
                    var history = await _messageRepository.GetByConversationIdAsync(conversationId, 50);

                    // Get AI response
                    var aiResponseText = await _aiAssistantService.GetAiResponseAsync(history, userId);

                    // Create AI message
                    var aiMessageId = Guid.NewGuid().ToString();
                    var aiMessage = new Message(
                        aiMessageId,
                        conversationId,
                        aiParticipant.UserId,
                        aiResponseText,
                        DateTime.UtcNow
                    );

                    await _messageRepository.AddAsync(aiMessage);

                    // Update the conversation's last message time
                    conversation.UpdateLastMessageTime(aiMessage.Timestamp);
                    await _conversationRepository.UpdateAsync(conversation);

                    // Notify that AI stopped typing
                    await _notificationService.NotifyUserStoppedTyping(conversationId, aiUserDto);

                    // Notify about the new message
                    var aiMessageDto = await MapToMessageDtoAsync(aiMessage);
                    await _notificationService.NotifyMessageReceived(conversationId, aiMessageDto);

                    _logger.LogInformation("AI assistant sent response {MessageId} in conversation {ConversationId}",
                        aiMessageId, conversationId);

                    return aiMessageDto;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating AI response in conversation {ConversationId}", conversationId);

                    // Create a system alert message about the error
                    var errorMessage = Message.CreateSystemAlert(
                        conversationId,
                        "Sorry, there was an error generating the AI response. Please try again later."
                    );
                    await _messageRepository.AddAsync(errorMessage);

                    // Update the conversation's last message time
                    conversation.UpdateLastMessageTime(errorMessage.Timestamp);
                    await _conversationRepository.UpdateAsync(conversation);

                    // Notify about the error message
                    var errorMessageDto = await MapToMessageDtoAsync(errorMessage);
                    await _notificationService.NotifyMessageReceived(conversationId, errorMessageDto);

                    return userMessageDto;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to AI in conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Regenerates an AI assistant response
        /// </summary>
        public async Task<MessageDto> RegenerateAiResponseAsync(string conversationId, string messageId, string userId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentNullException(nameof(messageId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                // Check authorization
                var isAuthorized = await _authService.IsUserAuthorizedForConversationAsync(userId, conversationId);
                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to access conversation {conversationId}");
                }

                // Get the message to regenerate
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null || message.ConversationId != conversationId)
                {
                    throw new ArgumentException($"Message {messageId} not found in conversation {conversationId}");
                }

                // Check if this is an AI conversation
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation.Type != ConversationType.AiAssistant)
                {
                    throw new InvalidOperationException($"Conversation {conversationId} is not an AI assistant conversation");
                }

                // Find the AI assistant participant
                var aiParticipant = conversation.Participants.FirstOrDefault(p => p.Role == ParticipantRole.AiAssistant);
                if (aiParticipant == null || message.AuthorId != aiParticipant.UserId)
                {
                    throw new InvalidOperationException("The specified message is not from the AI assistant");
                }

                // Check if AI request limit is reached
                if (await _aiAssistantService.IsRequestLimitReachedAsync(userId))
                {
                    // Create a system message indicating the rate limit
                    var systemMessage = Message.CreateSystemAlert(
                        conversationId,
                        "Request limit reached. Please try again later."
                    );
                    await _messageRepository.AddAsync(systemMessage);

                    // Update the conversation's last message time
                    conversation.UpdateLastMessageTime(systemMessage.Timestamp);
                    await _conversationRepository.UpdateAsync(conversation);

                    // Notify about the system message
                    var systemMessageDto = await MapToMessageDtoAsync(systemMessage);
                    await _notificationService.NotifyMessageReceived(conversationId, systemMessageDto);

                    throw new InvalidOperationException("Request limit reached. Please try again later.");
                }

                // Mark the message as being regenerated
                message.MarkAsRegenerated();
                await _messageRepository.UpdateAsync(message);

                // Notify clients that regeneration has started
                var aiUser = await _userRepository.GetByIdAsync(aiParticipant.UserId);
                var regeneratingDto = new MessageDto
                {
                    Id = message.Id,
                    Text = "Regenerating...",
                    Timestamp = message.Timestamp,
                    Author = MapToUserDto(aiUser),
                    IsBeingRegenerated = true
                };

                await _notificationService.NotifyMessageReceived(conversationId, regeneratingDto);

                try
                {
                    // Get conversation history up to the message being regenerated
                    var history = (await _messageRepository.GetByConversationIdAsync(conversationId, 50))
                        .Where(m => m.Timestamp < message.Timestamp)
                        .ToList();

                    // Get the new AI response
                    var newText = await _aiAssistantService.RegenerateResponseAsync(history, messageId);

                    // Update the message
                    message.CompleteRegeneration(newText);
                    await _messageRepository.UpdateAsync(message);

                    // Notify clients of the completed regeneration
                    var completedDto = await MapToMessageDtoAsync(message);
                    await _notificationService.NotifyMessageReceived(conversationId, completedDto);

                    _logger.LogInformation("Regenerated AI response {MessageId} in conversation {ConversationId}",
                        messageId, conversationId);

                    return completedDto;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error regenerating AI response {MessageId}", messageId);

                    // Revert the message to its original state
                    message.CompleteRegeneration(message.Text);
                    await _messageRepository.UpdateAsync(message);

                    // Create a system alert about the error
                    var errorMessage = Message.CreateSystemAlert(
                        conversationId,
                        "Sorry, there was an error regenerating the AI response. Please try again later."
                    );
                    await _messageRepository.AddAsync(errorMessage);

                    // Notify about the error message
                    var errorMessageDto = await MapToMessageDtoAsync(errorMessage);
                    await _notificationService.NotifyMessageReceived(conversationId, errorMessageDto);

                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating AI response {MessageId} in conversation {ConversationId}",
                    messageId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Edits a message
        /// </summary>
        public async Task<MessageDto> EditMessageAsync(string messageId, string userId, string newText)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentNullException(nameof(messageId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(newText))
            {
                throw new ArgumentException("New message text cannot be empty", nameof(newText));
            }

            try
            {
                // Retrieve the message
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null)
                {
                    throw new ArgumentException($"Message with ID {messageId} does not exist");
                }

                // Verify the user is the author of the message
                if (message.AuthorId != userId)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not the author of message {messageId}");
                }

                // Update the message text
                message.EditText(newText);
                await _messageRepository.UpdateAsync(message);

                // Notify participants of the edit
                var messageDto = await MapToMessageDtoAsync(message);
                await _notificationService.NotifyMessageReceived(message.ConversationId, messageDto);

                _logger.LogInformation("User {UserId} edited message {MessageId} in conversation {ConversationId}",
                    userId, messageId, message.ConversationId);

                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message {MessageId}", messageId);
                throw;
            }
        }

        /// <summary>
        /// Notifies that a user has started typing in a conversation
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
                await _typingService.UserStartedTypingAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording typing start for user {UserId} in conversation {ConversationId}",
                    userId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Notifies that a user has stopped typing in a conversation
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
                await _typingService.UserStoppedTypingAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording typing stop for user {UserId} in conversation {ConversationId}",
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
                return await _typingService.GetUsersTypingAsync(conversationId, excludeUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users typing in conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Archives a conversation for a user
        /// </summary>
        public async Task ArchiveConversationAsync(string conversationId, string userId)
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
                await _conversationRepository.ArchiveAsync(conversationId, userId);
                _logger.LogInformation("User {UserId} archived conversation {ConversationId}", userId, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving conversation {ConversationId} for user {UserId}", conversationId, userId);
                throw;
            }
        }

        /// <summary>
        /// Unarchives a conversation for a user
        /// </summary>
        public async Task UnarchiveConversationAsync(string conversationId, string userId)
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
                await _conversationRepository.UnarchiveAsync(conversationId, userId);
                _logger.LogInformation("User {UserId} unarchived conversation {ConversationId}", userId, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unarchiving conversation {ConversationId} for user {UserId}", conversationId, userId);
                throw;
            }
        }

        /// <summary>
        /// Adds a participant to a conversation
        /// </summary>
        public async Task AddParticipantAsync(string conversationId, string userId, string participantId, ParticipantRole role = ParticipantRole.Member)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(participantId))
            {
                throw new ArgumentNullException(nameof(participantId));
            }

            try
            {
                // Check if the user has permissions to add participants
                var canAdd = await _authService.CanAddParticipantsAsync(userId, conversationId);
                if (!canAdd)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to add participants to conversation {conversationId}");
                }

                // Add the participant
                var success = await _conversationRepository.AddParticipantAsync(conversationId, participantId, role);
                if (!success)
                {
                    throw new InvalidOperationException($"Failed to add participant {participantId} to conversation {conversationId}");
                }

                // Create a system message about the new participant
                var participant = await _userRepository.GetByIdAsync(participantId);
                var systemMessage = Message.CreateSystemAlert(
                    conversationId,
                    $"{participant.Name} joined the conversation."
                );
                await _messageRepository.AddAsync(systemMessage);

                // Update the conversation's last message time
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                conversation.UpdateLastMessageTime(systemMessage.Timestamp);
                await _conversationRepository.UpdateAsync(conversation);

                // Notify about the system message
                var systemMessageDto = await MapToMessageDtoAsync(systemMessage);
                await _notificationService.NotifyMessageReceived(conversationId, systemMessageDto);

                // Notify about participant changes
                var updatedConversation = await GetConversationByIdAsync(conversationId, userId);
                await _notificationService.NotifyParticipantsChanged(conversationId, updatedConversation);

                _logger.LogInformation("User {UserId} added participant {ParticipantId} to conversation {ConversationId}",
                    userId, participantId, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding participant {ParticipantId} to conversation {ConversationId}",
                    participantId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Removes a participant from a conversation
        /// </summary>
        public async Task RemoveParticipantAsync(string conversationId, string userId, string participantId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(participantId))
            {
                throw new ArgumentNullException(nameof(participantId));
            }

            try
            {
                // Check if the user has permissions to remove participants
                var canRemove = await _authService.CanRemoveParticipantsAsync(userId, conversationId, participantId);
                if (!canRemove)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to remove participant {participantId} from conversation {conversationId}");
                }

                // Get participant name before removal
                var participant = await _userRepository.GetByIdAsync(participantId);
                var participantName = participant?.Name ?? "User";

                // Remove the participant
                var success = await _conversationRepository.RemoveParticipantAsync(conversationId, participantId);
                if (!success)
                {
                    throw new InvalidOperationException($"Failed to remove participant {participantId} from conversation {conversationId}");
                }

                // Create a system message about the removed participant
                var systemMessage = Message.CreateSystemAlert(
                    conversationId,
                    $"{participantName} left the conversation."
                );
                await _messageRepository.AddAsync(systemMessage);

                // Update the conversation's last message time
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                conversation.UpdateLastMessageTime(systemMessage.Timestamp);
                await _conversationRepository.UpdateAsync(conversation);

                // Notify about the system message
                var systemMessageDto = await MapToMessageDtoAsync(systemMessage);
                await _notificationService.NotifyMessageReceived(conversationId, systemMessageDto);

                // Notify about participant changes
                var updatedConversation = await GetConversationByIdAsync(conversationId, userId);
                await _notificationService.NotifyParticipantsChanged(conversationId, updatedConversation);

                _logger.LogInformation("User {UserId} removed participant {ParticipantId} from conversation {ConversationId}",
                    userId, participantId, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing participant {ParticipantId} from conversation {ConversationId}",
                    participantId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Updates a participant's role in a conversation
        /// </summary>
        public async Task UpdateParticipantRoleAsync(string conversationId, string userId, string participantId, ParticipantRole newRole)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(participantId))
            {
                throw new ArgumentNullException(nameof(participantId));
            }

            try
            {
                // Check if the user has permissions to update participant roles
                var canUpdate = await _authService.CanUpdateConversationAsync(userId, conversationId);
                if (!canUpdate)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to update participant roles in conversation {conversationId}");
                }

                // Get the conversation
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation {conversationId} not found");
                }

                // Find the participant
                var participant = conversation.Participants.FirstOrDefault(p => p.UserId == participantId);
                if (participant == null)
                {
                    throw new KeyNotFoundException($"Participant {participantId} not found in conversation {conversationId}");
                }

                // Update the role
                participant.UpdateRole(newRole);
                await _conversationRepository.UpdateParticipantAsync(participant);

                // Create a system message about the role change
                var participantUser = await _userRepository.GetByIdAsync(participantId);
                var participantName = participantUser?.Name ?? "User";
                var systemMessage = Message.CreateSystemAlert(
                    conversationId,
                    $"{participantName} is now a {newRole.ToString().ToLower()} of the conversation."
                );
                await _messageRepository.AddAsync(systemMessage);

                // Update the conversation's last message time
                conversation.UpdateLastMessageTime(systemMessage.Timestamp);
                await _conversationRepository.UpdateAsync(conversation);

                // Notify about the system message
                var systemMessageDto = await MapToMessageDtoAsync(systemMessage);
                await _notificationService.NotifyMessageReceived(conversationId, systemMessageDto);

                // Notify about participant changes
                var updatedConversation = await GetConversationByIdAsync(conversationId, userId);
                await _notificationService.NotifyParticipantsChanged(conversationId, updatedConversation);

                _logger.LogInformation("User {UserId} updated participant {ParticipantId} role to {Role} in conversation {ConversationId}",
                    userId, participantId, newRole, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating participant {ParticipantId} role in conversation {ConversationId}",
                    participantId, conversationId);
                throw;
            }
        }

        /// <summary>
        /// Marks all messages in a conversation as read by a user up to the current time
        /// </summary>
        public async Task MarkConversationAsReadAsync(string conversationId, string userId)
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
                // Check authorization
                var isAuthorized = await _authService.IsUserAuthorizedForConversationAsync(userId, conversationId);
                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to access conversation {conversationId}");
                }

                // Mark messages as read up to the current time
                var now = DateTime.UtcNow;
                var count = await _readReceiptService.MarkMessagesAsReadAsync(conversationId, userId, now);

                // Get updated read receipts
                var readReceipts = await _readReceiptService.GetReadReceiptsForConversationAsync(conversationId);

                // Notify about updated read receipts
                await _notificationService.NotifyReadReceiptsUpdated(conversationId, readReceipts);

                _logger.LogInformation("User {UserId} marked {Count} messages as read in conversation {ConversationId}",
                    userId, count, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation {ConversationId} as read for user {UserId}",
                    conversationId, userId);
                throw;
            }
        }

        /// <summary>
        /// Gets read receipts for a conversation (user IDs mapped to their last read timestamps)
        /// </summary>
        public async Task<IDictionary<string, DateTime>> GetReadReceiptsAsync(string conversationId, string userId)
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
                // Check authorization
                var isAuthorized = await _authService.IsUserAuthorizedForConversationAsync(userId, conversationId);
                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException($"User {userId} is not authorized to access conversation {conversationId}");
                }

                return await _readReceiptService.GetReadReceiptsForConversationAsync(conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting read receipts for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Helper method to get participant DTOs for a conversation
        /// </summary>
        private async Task<List<UserDto>> GetParticipantDtosAsync(Conversation conversation)
        {
            var participantDtos = new List<UserDto>();

            foreach (var participant in conversation.Participants)
            {
                var user = await _userRepository.GetByIdAsync(participant.UserId);
                if (user != null)
                {
                    participantDtos.Add(MapToUserDto(user));
                }
            }

            return participantDtos;
        }

        /// <summary>
        /// Maps a Message entity to a MessageDto
        /// </summary>
        private async Task<MessageDto> MapToMessageDtoAsync(Message message)
        {
            if (message == null)
            {
                return null;
            }

            var author = await _userRepository.GetByIdAsync(message.AuthorId);

            return new MessageDto
            {
                Id = message.Id,
                Text = message.Text,
                Timestamp = message.Timestamp,
                Author = MapToUserDto(author),
                IsEdited = message.IsEdited,
                IsBeingRegenerated = message.IsBeingRegenerated,
                IsSystemAlert = message.IsSystemAlert,
                ParentMessageId = message.ParentMessageId
            };
        }

        /// <summary>
        /// Maps a collection of Message entities to MessageDtos
        /// </summary>
        private async Task<IEnumerable<MessageDto>> MapToMessageDtosAsync(IEnumerable<Message> messages)
        {
            var messageDtos = new List<MessageDto>();

            foreach (var message in messages)
            {
                messageDtos.Add(await MapToMessageDtoAsync(message));
            }

            return messageDtos;
        }

        /// <summary>
        /// Maps a User entity to a UserDto
        /// </summary>
        private UserDto MapToUserDto(User user)
        {
            if (user == null)
            {
                return new UserDto
                {
                    Id = "system",
                    Name = "System"
                };
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                AvatarAlt = user.AvatarAlt
            };
        }
    }
}
