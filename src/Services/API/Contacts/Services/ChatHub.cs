using API.Contacts.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.Contacts.Services;

/// <summary>
/// SignalR Hub for chat functionality
/// </summary>
public class ChatHub : Hub
{private readonly IConnectionManager _connectionManager;
    private readonly IOidcAuthenticationService _authService;
    private readonly IChatApplicationService _chatService;
    private readonly IUserService _userService;
    private readonly IMessageService _messageService;
    private readonly ITypingService _typingService;
    private readonly IAlertService _alertService;

    public ChatHub(
        IConnectionManager connectionManager,
        IOidcAuthenticationService authService,
        IChatApplicationService chatService,
        IUserService userService,
        IMessageService messageService,
        ITypingService typingService,
        IAlertService alertService)
    {
        _connectionManager = connectionManager;
        _authService = authService;
        _chatService = chatService;
        _userService = userService;
        _messageService = messageService;
        _typingService = typingService;
        _alertService = alertService;
    }

    public override async Task OnConnectedAsync()
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
        }
        else
        {
            throw new HubException("User is not authenticated");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _connectionManager.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific conversation channel
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        var userId = await GetUserIdFromContext();
        
        // Check if the user is authorized to join this conversation
        var isAuthorized = await _authService.IsUserAuthorizedForConversationAsync(userId, conversationId);

        if (!isAuthorized)
        {
            throw new HubException("User is not authorized to join this conversation");
        }

        _connectionManager.AddToConversation(Context.ConnectionId, conversationId);
        
        // Load previous messages
        var messages = await _messageService.GetConversationMessagesAsync(conversationId, 50);
        await Clients.Caller.SendAsync("LoadMessages", conversationId, messages);
        
        // Load typing users
        var typingUsers = await _typingService.GetUsersTypingAsync(conversationId, userId);
        if (typingUsers.Any())
        {
            await Clients.Caller.SendAsync("UsersTyping", conversationId, typingUsers);
        }
    }

    /// <summary>
    /// Leave a conversation channel
    /// </summary>
    public void LeaveConversation(string conversationId)
    {
        _connectionManager.RemoveFromConversation(Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Load all conversations for the current user
    /// </summary>
    public async Task<IEnumerable<ConversationDto>> LoadConversations()
    {
        var userId = await GetUserIdFromContext();
        var conversations = await _chatService.GetUserConversationsAsync(userId);
        return conversations.Select(MapToConversationDto).ToList();
    }

    /// <summary>
    /// Send a message to a conversation
    /// </summary>
    public async Task<MessageDto> SendMessage(string conversationId, string text, string parentMessageId = null)
    {
        var userId = await GetUserIdFromContext();
        
        var message = await _chatService.SendMessageAsync(conversationId, userId, text, parentMessageId);
        return MapToMessageDto(message);
    }

    /// <summary>
    /// Send a message to an AI assistant conversation
    /// </summary>
    public async Task<MessageDto> SendMessageToAi(string conversationId, string text)
    {
        var userId = await GetUserIdFromContext();
        
        var message = await _chatService.SendMessageToAiAsync(conversationId, userId, text);
        return MapToMessageDto(message);
    }

    /// <summary>
    /// Create a new conversation with specified participants
    /// </summary>
    public async Task<ConversationDto> CreateConversation(IEnumerable<string> participantIds, string title = null)
    {
        var userId = await GetUserIdFromContext();
        
        var conversation = await _chatService.CreateConversationAsync(userId, participantIds, title);
        return MapToConversationDto(conversation);
    }

    /// <summary>
    /// Create a new AI assistant conversation
    /// </summary>
    public async Task<ConversationDto> CreateAiAssistantConversation(string title = "AI Assistant")
    {
        var userId = await GetUserIdFromContext();
        
        var conversation = await _chatService.CreateAiAssistantConversationAsync(userId, title);
        return MapToConversationDto(conversation);
    }

    /// <summary>
    /// Regenerate an AI assistant response
    /// </summary>
    public async Task<MessageDto> RegenerateAiResponse(string conversationId, string messageId)
    {
        var userId = await GetUserIdFromContext();
        
        var message = await _chatService.RegenerateAiResponseAsync(conversationId, messageId, userId);
        return MapToMessageDto(message);
    }

    /// <summary>
    /// Indicate that the user has started typing
    /// </summary>
    public async Task UserStartedTyping(string conversationId)
    {
        var userId = await GetUserIdFromContext();
        await _chatService.UserStartedTypingAsync(conversationId, userId);
    }

    /// <summary>
    /// Indicate that the user has stopped typing
    /// </summary>
    public async Task UserStoppedTyping(string conversationId)
    {
        var userId = await GetUserIdFromContext();
        await _chatService.UserStoppedTypingAsync(conversationId, userId);
    }

    /// <summary>
    /// Mark messages as read up to the current timestamp
    /// </summary>
    public async Task MarkAsRead(string conversationId)
    {
        var userId = await GetUserIdFromContext();
        await _messageService.MarkMessagesAsReadAsync(conversationId, userId, DateTime.UtcNow);
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

    /// <summary>
    /// Map Conversation entity to DTO
    /// </summary>
    private ConversationDto MapToConversationDto(Conversation conversation)
    {
        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Type = conversation.Type,
            Participants = conversation.Participants?
                .Select(p => new UserDto 
                { 
                    Id = p.User.Id,
                    Name = p.User.Name,
                    AvatarUrl = p.User.AvatarUrl,
                    AvatarAlt = p.User.AvatarAlt
                })
                .ToList() ?? new List<UserDto>(),
            LastMessageAt = conversation.LastMessageAt,
            IsArchived = conversation.IsArchived
        };
    }

    /// <summary>
    /// Map Message entity to DTO
    /// </summary>
    private MessageDto MapToMessageDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            Text = message.Text,
            Timestamp = message.Timestamp,
            Author = new UserDto 
            { 
                Id = message.Author.Id,
                Name = message.Author.Name,
                AvatarUrl = message.Author.AvatarUrl,
                AvatarAlt = message.Author.AvatarAlt
            },
            IsEdited = message.IsEdited,
            IsBeingRegenerated = message.IsBeingRegenerated,
            ParentMessageId = message.ParentMessageId
        };
    }
}
