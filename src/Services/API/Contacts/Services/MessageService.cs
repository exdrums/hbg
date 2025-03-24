using API.Contacts.Data.Repositories;
using API.Contacts.Dtos;
using API.Contacts.Model;
using API.Contacts.Services.Interfaces;

namespace API.Contacts.Services;

/// <summary>
/// Implementation of the Message Service
/// </summary>
public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly IAiAssistantService _aiAssistantService;
    
    // In a real implementation, you would inject a proper logger and event publisher
    // private readonly ILogger<MessageService> _logger;
    // private readonly IEventPublisher _eventPublisher;
    
    public MessageService(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IRealtimeNotificationService realtimeNotificationService,
        IAiAssistantService aiAssistantService)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _realtimeNotificationService = realtimeNotificationService ?? throw new ArgumentNullException(nameof(realtimeNotificationService));
        _aiAssistantService = aiAssistantService ?? throw new ArgumentNullException(nameof(aiAssistantService));
    }

    /// <summary>
    /// Sends a new message in a conversation
    /// </summary>
    public async Task<Message> SendMessageAsync(string conversationId, string userId, string text, string parentMessageId = null)
    {
        // Verify the conversation exists and user is a participant
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null)
        {
            throw new ArgumentException($"Conversation with ID {conversationId} does not exist");
        }

        var isParticipant = conversation.Participants.Any(p => p.UserId == userId);
        if (!isParticipant)
        {
            throw new UnauthorizedAccessException($"User {userId} is not a participant in conversation {conversationId}");
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
        
        var success = await _messageRepository.AddAsync(message);
        if (!success)
        {
            throw new Exception($"Failed to save message to the repository");
        }

        // Update the conversation's last message time
        conversation.UpdateLastMessageTime(message.Timestamp);
        await _conversationRepository.UpdateAsync(conversation);

        // Publish the message sent event
        // In a real implementation, this would use a proper event publisher
        var messageSentEvent = new MessageSentEvent(messageId, conversationId, userId);
        // _eventPublisher.Publish(messageSentEvent);

        // Notify other participants in real-time
        var user = await _userRepository.GetByIdAsync(userId);
        var messageDto = new MessageDto
        {
            Id = message.Id,
            Text = message.Text,
            Timestamp = message.Timestamp,
            Author = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                AvatarAlt = user.AvatarAlt
            },
            ParentMessageId = message.ParentMessageId
        };

        await _realtimeNotificationService.NotifyMessageReceived(conversationId, messageDto);

        // If this is an AI Assistant conversation, trigger AI response
        if (conversation.Type == ConversationType.AiAssistant)
        {
            await TriggerAiResponseAsync(conversation, message);
        }

        return message;
    }

    /// <summary>
    /// Edits an existing message
    /// </summary>
    public async Task<Message> EditMessageAsync(string messageId, string userId, string newText)
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

        // Don't allow editing system alert messages
        if (message.IsSystemAlert)
        {
            throw new InvalidOperationException("System alert messages cannot be edited");
        }

        // Don't allow editing messages that are being regenerated
        if (message.IsBeingRegenerated)
        {
            throw new InvalidOperationException("Messages that are being regenerated cannot be edited");
        }

        // Update the message text
        message.EditText(newText);
        
        var success = await _messageRepository.UpdateAsync(message);
        if (!success)
        {
            throw new Exception($"Failed to update message in the repository");
        }

        // Notify participants of the edit
        var conversation = await _conversationRepository.GetByIdAsync(message.ConversationId);
        var user = await _userRepository.GetByIdAsync(userId);
        
        var messageDto = new MessageDto
        {
            Id = message.Id,
            Text = message.Text,
            Timestamp = message.Timestamp,
            Author = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                AvatarAlt = user.AvatarAlt
            },
            IsEdited = true,
            ParentMessageId = message.ParentMessageId
        };

        await _realtimeNotificationService.NotifyMessageReceived(message.ConversationId, messageDto);

        return message;
    }

    /// <summary>
    /// Retrieves messages from a conversation
    /// </summary>
    public async Task<IEnumerable<Message>> GetConversationMessagesAsync(string conversationId, int limit = 50, DateTime? before = null)
    {
        // Verify the conversation exists
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null)
        {
            throw new ArgumentException($"Conversation with ID {conversationId} does not exist");
        }

        // Retrieve messages
        var messages = await _messageRepository.GetByConversationIdAsync(conversationId, limit, before);
        
        return messages;
    }

    /// <summary>
    /// Regenerates an AI message
    /// </summary>
    public async Task<Message> RegenerateAiMessageAsync(string messageId, string userId)
    {
        // Retrieve the message
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
        {
            throw new ArgumentException($"Message with ID {messageId} does not exist");
        }

        // Verify this is an AI message
        var conversation = await _conversationRepository.GetByIdAsync(message.ConversationId);
        if (conversation.Type != ConversationType.AiAssistant)
        {
            throw new InvalidOperationException("Only AI assistant messages can be regenerated");
        }

        // Find the AI participant
        var aiParticipant = conversation.Participants
            .FirstOrDefault(p => p.Role == ParticipantRole.AiAssistant);
        
        if (aiParticipant == null || message.AuthorId != aiParticipant.UserId)
        {
            throw new InvalidOperationException("The specified message is not from the AI assistant");
        }

        // Verify the requesting user is a participant
        var isParticipant = conversation.Participants.Any(p => p.UserId == userId);
        if (!isParticipant)
        {
            throw new UnauthorizedAccessException($"User {userId} is not a participant in conversation {message.ConversationId}");
        }

        // Check if AI request limit is reached
        if (await _aiAssistantService.IsRequestLimitReachedAsync(userId))
        {
            throw new InvalidOperationException("Request limit reached. Please try again later.");
        }

        // Mark the message as being regenerated
        message.MarkAsRegenerated();
        await _messageRepository.UpdateAsync(message);

        // Notify participants that regeneration has started
        var aiUser = await _userRepository.GetByIdAsync(aiParticipant.UserId);
        var regeneratingDto = new MessageDto
        {
            Id = message.Id,
            Text = "Regenerating...",
            Timestamp = message.Timestamp,
            Author = new UserDto
            {
                Id = aiUser.Id,
                Name = aiUser.Name,
                AvatarUrl = aiUser.AvatarUrl,
                AvatarAlt = aiUser.AvatarAlt
            },
            IsBeingRegenerated = true
        };

        await _realtimeNotificationService.NotifyMessageReceived(message.ConversationId, regeneratingDto);

        try
        {
            // Get conversation history up to the message being regenerated
            var history = (await _messageRepository.GetByConversationIdAsync(message.ConversationId, 50))
                .Where(m => m.Timestamp < message.Timestamp)
                .ToList();

            // Get the new AI response
            var newText = await _aiAssistantService.RegenerateResponseAsync(history, messageId);

            // Update the message
            message.CompleteRegeneration(newText);
            await _messageRepository.UpdateAsync(message);

            // Notify participants of the new message
            var completedDto = new MessageDto
            {
                Id = message.Id,
                Text = message.Text,
                Timestamp = message.Timestamp,
                Author = new UserDto
                {
                    Id = aiUser.Id,
                    Name = aiUser.Name,
                    AvatarUrl = aiUser.AvatarUrl,
                    AvatarAlt = aiUser.AvatarAlt
                },
                IsEdited = true
            };

            await _realtimeNotificationService.NotifyMessageReceived(message.ConversationId, completedDto);

            return message;
        }
        catch (Exception ex)
        {
            // If regeneration fails, revert the message to its original state
            message.CompleteRegeneration(message.Text);
            await _messageRepository.UpdateAsync(message);

            // Log the error
            // _logger.LogError(ex, "Error regenerating AI response");

            // Create a system alert about the error
            await CreateSystemAlertMessageAsync(message.ConversationId, 
                "Sorry, there was an error regenerating the AI response. Please try again later.");

            // Re-throw the exception for the caller to handle
            throw;
        }
    }

    /// <summary>
    /// Creates a system alert message in a conversation
    /// </summary>
    public async Task<Message> CreateSystemAlertMessageAsync(string conversationId, string alertText)
    {
        // Verify the conversation exists
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null)
        {
            throw new ArgumentException($"Conversation with ID {conversationId} does not exist");
        }

        // Create the system alert message
        var message = Message.CreateSystemAlert(conversationId, alertText);
        
        var success = await _messageRepository.AddAsync(message);
        if (!success)
        {
            throw new Exception($"Failed to save system alert message to the repository");
        }

        // No need to update the conversation's last message time for system alerts

        // Notify participants of the system alert
        var messageDto = new MessageDto
        {
            Id = message.Id,
            Text = message.Text,
            Timestamp = message.Timestamp,
            Author = new UserDto
            {
                Id = "system",
                Name = "System"
            }
        };

        await _realtimeNotificationService.NotifyMessageReceived(conversationId, messageDto);

        return message;
    }

    /// <summary>
    /// Marks messages as read by a user up to a certain timestamp
    /// </summary>
    public async Task MarkMessagesAsReadAsync(string conversationId, string userId, DateTime timestamp)
    {
        // Verify the conversation exists and user is a participant
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null)
        {
            throw new ArgumentException($"Conversation with ID {conversationId} does not exist");
        }

        var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
        {
            throw new UnauthorizedAccessException($"User {userId} is not a participant in conversation {conversationId}");
        }

        // Update the last read timestamp
        participant.UpdateLastRead(timestamp);
        
        var success = await _conversationRepository.UpdateParticipantAsync(participant);
        if (!success)
        {
            throw new Exception($"Failed to update participant read receipt in the repository");
        }

        // No need to notify other participants about read receipts in this implementation
        // In a real application, you might want to broadcast this to show read receipts

        return;
    }

    /// <summary>
    /// Helper method to trigger an AI response
    /// </summary>
    private async Task TriggerAiResponseAsync(Conversation conversation, Message userMessage)
    {
        try
        {
            // Find the AI assistant participant
            var aiParticipant = conversation.Participants.FirstOrDefault(p => p.Role == ParticipantRole.AiAssistant);
            if (aiParticipant == null)
            {
                // No AI assistant in the conversation
                return;
            }

            // Get conversation history
            var history = await _messageRepository.GetByConversationIdAsync(conversation.Id, 50);
            
            // Check if AI request limit is reached
            var userId = userMessage.AuthorId;
            if (await _aiAssistantService.IsRequestLimitReachedAsync(userId))
            {
                // Create a system message indicating rate limit
                await CreateSystemAlertMessageAsync(conversation.Id, 
                    "Request limit reached. Please try again later.");
                return;
            }

            // Notify that AI is "typing"
            var aiUser = await _userRepository.GetByIdAsync(aiParticipant.UserId);
            var aiUserDto = new UserDto
            {
                Id = aiUser.Id,
                Name = aiUser.Name,
                AvatarUrl = aiUser.AvatarUrl,
                AvatarAlt = aiUser.AvatarAlt
            };
            
            await _realtimeNotificationService.NotifyUserStartedTyping(conversation.Id, aiUserDto);

            // Get AI response
            var aiResponseText = await _aiAssistantService.GetAiResponseAsync(history, userId);

            // Create AI message
            var aiMessageId = Guid.NewGuid().ToString();
            var aiMessage = new Message(
                aiMessageId,
                conversation.Id,
                aiParticipant.UserId,
                aiResponseText,
                DateTime.UtcNow
            );

            await _messageRepository.AddAsync(aiMessage);
            
            // Update the conversation's last message time
            conversation.UpdateLastMessageTime(aiMessage.Timestamp);
            await _conversationRepository.UpdateAsync(conversation);

            // Notify that AI stopped typing
            await _realtimeNotificationService.NotifyUserStoppedTyping(conversation.Id, aiUserDto);

            // Notify about the new message
            var aiMessageDto = new MessageDto
            {
                Id = aiMessage.Id,
                Text = aiMessage.Text,
                Timestamp = aiMessage.Timestamp,
                Author = aiUserDto
            };

            await _realtimeNotificationService.NotifyMessageReceived(conversation.Id, aiMessageDto);
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error generating AI response");
            
            // Create a system alert message about the error
            await CreateSystemAlertMessageAsync(conversation.Id, 
                "Sorry, there was an error generating the AI response. Please try again later.");
        }
    }
}
