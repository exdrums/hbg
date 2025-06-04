using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using API.Contacts.Services.Interfaces;
using API.Contacts.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using Common.Utils;

namespace API.Contacts.WebSocket
{
    /// <summary>
    /// Enhanced SignalR Hub implementation for real-time chat functionality
    /// 
    /// New features in this version:
    /// 1. Integrated alert system for non-persistent notifications
    /// 2. Support for different conversation types (Contacts, Support, Agent)
    /// 3. Role-based access control and notifications
    /// 4. Enhanced error handling and permission checks
    /// 5. Better integration with DevExtreme DataSources
    /// 
    /// This hub serves as the main communication gateway between clients and
    /// the chat microservice, orchestrating real-time messaging, presence,
    /// and notification delivery.
    /// </summary>
    [Authorize] // Requires authentication for all hub methods
    public class ChatHub : Hub<IChatHubClient>, IChatHub
    {
        private readonly IConversationService _conversationService;
        private readonly IMessageService _messageService;
        private readonly IUserConnectionService _connectionService;
        private readonly IAlertService _alertService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IConversationService conversationService,
            IMessageService messageService,
            IUserConnectionService connectionService,
            IAlertService alertService,
            ILogger<ChatHub> logger)
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _connectionService = connectionService;
            _alertService = alertService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current user's ID from the authentication context
        /// </summary>
        private string CurrentUserId => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User ID not found in claims");

        /// <summary>
        /// Gets the current user's display name from claims
        /// </summary>
        private string CurrentUserName => Context.User?.FindFirst(ClaimTypes.Name)?.Value 
            ?? Context.User?.FindFirst("name")?.Value 
            ?? "Unknown User";

        /// <summary>
        /// Checks if current user has a specific claim/role
        /// </summary>
        private bool HasClaim(string claimType) => Context.User?.HasClaim("role", claimType) ?? false;

        #region Connection Lifecycle

        /// <summary>
        /// Called when a client establishes a connection to the hub
        /// Enhanced with role-based group management and alert system integration
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = CurrentUserId;
            var connectionId = Context.ConnectionId;

            _logger.LogInformation($"User {userId} connected with connection {connectionId}");

            try
            {
                // Track the user's connection for presence management
                await _connectionService.AddConnectionAsync(userId, connectionId);

                // Add to role-based groups for targeted notifications
                await JoinRoleBasedGroups(connectionId);

                // Get all conversations for this user and join the SignalR groups
                var userConversations = await _conversationService.GetUserConversationsAsync(userId);

                foreach (var conversation in userConversations)
                {
                    await Groups.AddToGroupAsync(connectionId, $"conversation_{conversation.ConversationId}");
                    // Send connection established alert
                    // await _alertService.SendConnectionEstablishedAlertAsync(userId);
                }

                // Notify other users that this user is now online
                await Clients.Others.UserStatusChanged(userId, UserStatus.Online);

                _logger.LogInformation($"User {userId} successfully connected and joined {userConversations.Count()} conversation groups");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during connection setup for user {userId}");
                await _alertService.SendConnectionLostAlertAsync(userId);
                throw;
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// Enhanced with better cleanup and notification handling
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = CurrentUserId;
            var connectionId = Context.ConnectionId;

            if (exception != null)
            {
                _logger.LogError(exception, $"User {userId} disconnected with error");
            }
            else
            {
                _logger.LogInformation($"User {userId} disconnected normally");
            }

            try
            {
                // Remove the connection tracking
                var hasMoreConnections = await _connectionService.RemoveConnectionAsync(userId, connectionId);

                // If this was the user's last connection, mark them as offline
                if (!hasMoreConnections)
                {
                    await Clients.Others.UserStatusChanged(userId, UserStatus.Offline);
                    
                    // Clear any typing indicators for this user
                    var userConversations = await _conversationService.GetUserConversationsAsync(userId);
                    foreach (var conversation in userConversations)
                    {
                        await Clients.Group($"conversation_{conversation.ConversationId}")
                            .UserStoppedTyping(conversation.ConversationId, userId);
                    }

                    _logger.LogInformation($"User {userId} marked as offline");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during disconnection cleanup for user {userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Joins user to role-based SignalR groups for targeted notifications
        /// </summary>
        private async Task JoinRoleBasedGroups(string connectionId)
        {
            // Add to support group if user has support role
            if (HasClaim("hbg-chat-support"))
            {
                await Groups.AddToGroupAsync(connectionId, "role_hbg-chat-support");
                _logger.LogDebug($"Added connection {connectionId} to support role group");
            }

            // Add to general users group (for system-wide notifications)
            await Groups.AddToGroupAsync(connectionId, "role_user");
        }

        #endregion

        #region Conversation Operations

        /// <summary>
        /// Loads conversations for the current user with support for different conversation types
        /// Enhanced with filtering by conversation type for tab-based UI
        /// </summary>
        public async Task<LoadResult> LoadConversation(DevExtremeLoadOptions loadOptions, string conversationType = "Contacts")
        {
            var userId = CurrentUserId;
            
            try
            {
                _logger.LogDebug($"Loading {conversationType} conversations for user {userId}");

                var conversations = await _conversationService.GetUserConversationsAsync(userId);
                
                // Filter conversations based on type for different tabs
                var filteredConversations = FilterConversationsByType(conversations, conversationType, userId);
                
                var loadResult = DataSourceLoader.Load(filteredConversations, loadOptions);
                
                _logger.LogInformation($"Loaded {filteredConversations.Count()} {conversationType} conversations for user {userId}");
                
                return loadResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading {conversationType} conversations for user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, $"Failed to load {conversationType} conversations");
                throw;
            }
        }

        /// <summary>
        /// Creates a new conversation with enhanced support for different conversation types
        /// </summary>
        public async Task<Conversation> CreateConversation(
            string title, 
            List<string> participantIds, 
            string conversationType = "Contacts")
        {
            var userId = CurrentUserId;
            
            try
            {
                // Ensure creator is included in participants
                if (!participantIds.Contains(userId))
                {
                    participantIds.Add(userId);
                }

                // Validate permissions based on conversation type
                await ValidateConversationCreationPermissions(conversationType, participantIds);

                // Create the conversation
                var conversation = await CreateConversationByType(userId, title, participantIds, conversationType);

                // Add all participants to the SignalR group
                var groupName = $"conversation_{conversation.ConversationId}";
                await AddParticipantsToGroup(participantIds, groupName);

                // Notify participants about the new conversation
                await Clients.Group(groupName).ConversationCreated(conversation);
                
                // Send DataSource update for the appropriate conversation type
                await Clients.Group(groupName).ConversationDataSourceInsert(conversation, conversationType);

                // Create appropriate system message
                var systemMessage = await CreateConversationSystemMessage(conversation, conversationType);
                if (systemMessage != null)
                {
                    await _messageService.SaveMessageAsync(systemMessage);
                    await Clients.Group(groupName).MessageReceived(systemMessage);
                }

                _logger.LogInformation($"Created {conversationType} conversation {conversation.ConversationId} by user {userId}");
                return conversation;
            }
            catch (UnauthorizedAccessException ex)
            {
                await _alertService.SendPermissionDeniedAlertAsync(userId, $"create {conversationType} conversation");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating {conversationType} conversation for user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, $"Failed to create {conversationType} conversation");
                throw;
            }
        }

        /// <summary>
        /// Updates conversation properties with enhanced validation and notifications
        /// </summary>
        public async Task UpdateConversation(Guid conversationId, Dictionary<string, object> updates)
        {
            var userId = CurrentUserId;
            
            try
            {
                if (!await _conversationService.UserHasAccessAsync(userId, conversationId))
                {
                    await _alertService.SendPermissionDeniedAlertAsync(userId, "update conversation", conversationId);
                    throw new UnauthorizedAccessException("You don't have access to this conversation");
                }

                var updatedConversation = await _conversationService.UpdateConversationAsync(conversationId, updates);

                // Notify all participants about the update
                await Clients.Group($"conversation_{conversationId}").ConversationUpdated(updatedConversation);

                // Create system messages for significant changes
                await HandleConversationUpdateNotifications(conversationId, updates);

                _logger.LogInformation($"Updated conversation {conversationId} by user {userId}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating conversation {conversationId} for user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, "Failed to update conversation", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Archives a conversation with enhanced notifications
        /// </summary>
        public async Task ArchiveConversation(Guid conversationId)
        {
            var userId = CurrentUserId;
            
            try
            {
                if (!await _conversationService.UserHasAccessAsync(userId, conversationId))
                {
                    await _alertService.SendPermissionDeniedAlertAsync(userId, "archive conversation", conversationId);
                    throw new UnauthorizedAccessException("You don't have access to this conversation");
                }

                await _conversationService.ArchiveConversationAsync(conversationId);
                
                // Notify all participants
                await Clients.Group($"conversation_{conversationId}").ConversationArchived(conversationId);
                
                // Send success alert to the user who archived it
                await _alertService.CreateSuccessAlertAsync(userId, "Conversation archived successfully");

                _logger.LogInformation($"Archived conversation {conversationId} by user {userId}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error archiving conversation {conversationId} for user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, "Failed to archive conversation", conversationId);
                throw;
            }
        }

        #endregion

        #region Message Operations

        /// <summary>
        /// Loads messages for a conversation with enhanced error handling
        /// </summary>
        public async Task<LoadResult> LoadMessages(Guid conversationId, DevExtremeLoadOptions loadOptions)
        {
            var userId = CurrentUserId;
            
            try
            {
                if (!await _conversationService.UserHasAccessAsync(userId, conversationId))
                {
                    await _alertService.SendPermissionDeniedAlertAsync(userId, "access conversation messages", conversationId);
                    throw new UnauthorizedAccessException("You don't have access to this conversation");
                }

                var messages = await _messageService.GetMessagesAsync(conversationId);
                var loadResult = DataSourceLoader.Load(messages, loadOptions);
                
                // Mark loaded messages as read
                await MarkLoadedMessagesAsRead(loadResult, userId);
                
                return loadResult;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading messages for conversation {conversationId}, user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, "Failed to load messages", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Sends a message with enhanced validation and error handling
        /// </summary>
        public async Task<Message> SendMessage(
            Guid conversationId, 
            string content, 
            MessageType type = MessageType.Text,
            string metadata = null,
            Guid? replyToMessageId = null)
        {
            var userId = CurrentUserId;
            
            try
            {
                if (!await _conversationService.UserHasAccessAsync(userId, conversationId))
                {
                    await _alertService.SendPermissionDeniedAlertAsync(userId, "send messages", conversationId);
                    throw new UnauthorizedAccessException("You don't have access to this conversation");
                }

                // Check if conversation is in read-only mode (e.g., closed support ticket)
                var conversation = await _conversationService.GetConversationAsync(conversationId);
                if (!conversation.IsActive)
                {
                    await _alertService.SendReadOnlyModeAlertAsync(userId, conversationId, "Conversation is archived");
                    throw new InvalidOperationException("Cannot send messages to archived conversation");
                }

                // Create and save the message
                var message = type switch
                {
                    MessageType.Text => Message.CreateTextMessage(conversationId, userId, content, replyToMessageId),
                    MessageType.File => throw new NotImplementedException("Use file upload endpoint for file messages"),
                    _ => throw new ArgumentException($"Unsupported message type: {type}")
                };

                await _messageService.SaveMessageAsync(message);
                await _conversationService.UpdateLastMessageAsync(conversationId, message);

                // Broadcast the message in real-time
                await Clients.Group($"conversation_{conversationId}").MessageReceived(message);

                _logger.LogInformation($"Message {message.MessageId} sent by user {userId} in conversation {conversationId}");
                return message;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message for user {userId} in conversation {conversationId}");
                await _alertService.CreateErrorAlertAsync(userId, "Failed to send message", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Edits a message with enhanced validation
        /// </summary>
        public async Task<bool> EditMessage(Guid messageId, string newContent)
        {
            var userId = CurrentUserId;
            
            try
            {
                var message = await _messageService.GetMessageAsync(messageId);
                if (message == null)
                {
                    await _alertService.CreateErrorAlertAsync(userId, "Message not found");
                    throw new ArgumentException("Message not found");
                }

                if (message.SenderUserId != userId)
                {
                    await _alertService.SendPermissionDeniedAlertAsync(userId, "edit message", message.ConversationId);
                    throw new UnauthorizedAccessException("You can only edit your own messages");
                }

                var success = message.Edit(newContent, userId);
                if (success)
                {
                    await _messageService.UpdateMessageAsync(message);
                    await Clients.Group($"conversation_{message.ConversationId}").MessageEdited(message);
                }

                return success;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error editing message {messageId} for user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, "Failed to edit message");
                throw;
            }
        }

        /// <summary>
        /// Deletes a message with enhanced validation
        /// </summary>
        public async Task<bool> DeleteMessage(Guid messageId)
        {
            var userId = CurrentUserId;
            
            try
            {
                var message = await _messageService.GetMessageAsync(messageId);
                if (message == null)
                {
                    await _alertService.CreateErrorAlertAsync(userId, "Message not found");
                    throw new ArgumentException("Message not found");
                }

                if (message.SenderUserId != userId)
                {
                    await _alertService.SendPermissionDeniedAlertAsync(userId, "delete message", message.ConversationId);
                    throw new UnauthorizedAccessException("You can only delete your own messages");
                }

                var success = message.Delete(userId);
                if (success)
                {
                    await _messageService.UpdateMessageAsync(message);
                    await Clients.Group($"conversation_{message.ConversationId}").MessageDeleted(messageId);
                }

                return success;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message {messageId} for user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, "Failed to delete message");
                throw;
            }
        }

        /// <summary>
        /// Marks a message as read with error handling
        /// </summary>
        public async Task MarkMessageAsRead(Guid messageId)
        {
            var userId = CurrentUserId;
            
            try
            {
                var message = await _messageService.GetMessageAsync(messageId);
                if (message == null) return;

                if (message.MarkAsReadBy(userId))
                {
                    await _messageService.UpdateMessageAsync(message);
                    await Clients.Group($"conversation_{message.ConversationId}")
                        .MessageReadReceiptUpdated(messageId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking message {messageId} as read for user {userId}");
                // Don't throw here as this is not critical
            }
        }

        #endregion

        #region Group Management

        /// <summary>
        /// Adds participants to a group conversation with enhanced validation
        /// </summary>
        public async Task AddParticipants(Guid conversationId, List<string> userIds)
        {
            var userId = CurrentUserId;
            
            try
            {
                var conversation = await _conversationService.GetConversationAsync(conversationId);
                
                if (conversation == null || conversation.Type != ConversationType.Group)
                {
                    await _alertService.CreateErrorAlertAsync(userId, "Can only add participants to group conversations", conversationId);
                    throw new ArgumentException("Can only add participants to group conversations");
                }

                foreach (var newUserId in userIds)
                {
                    if (await _conversationService.AddParticipantAsync(conversationId, newUserId))
                    {
                        // Add their connections to the SignalR group
                        var connections = await _connectionService.GetConnectionsAsync(newUserId);
                        foreach (var connectionId in connections)
                        {
                            await Groups.AddToGroupAsync(connectionId, $"conversation_{conversationId}");
                        }

                        // Create system message
                        var systemMessage = Message.CreateSystemMessage(
                            conversationId,
                            $"{CurrentUserName} added {newUserId} to the group");
                        
                        await _messageService.SaveMessageAsync(systemMessage);
                        await Clients.Group($"conversation_{conversationId}").MessageReceived(systemMessage);
                    }
                }

                // Notify about the updated conversation
                var updatedConversation = await _conversationService.GetConversationAsync(conversationId);
                await Clients.Group($"conversation_{conversationId}").ConversationUpdated(updatedConversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding participants to conversation {conversationId} for user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, "Failed to add participants", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Allows a user to leave a group conversation
        /// </summary>
        public async Task LeaveConversation(Guid conversationId)
        {
            var userId = CurrentUserId;
            
            try
            {
                var conversation = await _conversationService.GetConversationAsync(conversationId);
                
                if (conversation == null || conversation.Type != ConversationType.Group)
                {
                    await _alertService.CreateErrorAlertAsync(userId, "Can only leave group conversations", conversationId);
                    throw new ArgumentException("Can only leave group conversations");
                }

                if (await _conversationService.RemoveParticipantAsync(conversationId, userId))
                {
                    // Remove user's connections from the SignalR group
                    var connections = await _connectionService.GetConnectionsAsync(userId);
                    foreach (var connectionId in connections)
                    {
                        await Groups.RemoveFromGroupAsync(connectionId, $"conversation_{conversationId}");
                    }

                    // Create system message
                    var systemMessage = Message.CreateSystemMessage(
                        conversationId,
                        $"{CurrentUserName} left the group");
                    
                    await _messageService.SaveMessageAsync(systemMessage);
                    await Clients.Group($"conversation_{conversationId}").MessageReceived(systemMessage);

                    // Notify the leaving user
                    await Clients.Caller.ConversationLeft(conversationId);
                    
                    // Send success alert
                    await _alertService.CreateSuccessAlertAsync(userId, "You left the conversation");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving conversation {conversationId} for user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, "Failed to leave conversation", conversationId);
                throw;
            }
        }

        /// <summary>
        /// Removes a participant from a group conversation
        /// </summary>
        public async Task RemoveParticipant(Guid conversationId, string userIdToRemove)
        {
            var userId = CurrentUserId;
            
            // For now, users can only remove themselves
            if (userId != userIdToRemove)
            {
                await _alertService.SendPermissionDeniedAlertAsync(userId, "remove other participants", conversationId);
                throw new UnauthorizedAccessException("You can only remove yourself from the conversation");
            }

            await LeaveConversation(conversationId);
        }

        #endregion

        #region Typing Indicators

        /// <summary>
        /// Broadcasts that a user is typing
        /// </summary>
        public async Task StartTyping(Guid conversationId)
        {
            var userId = CurrentUserId;
            
            try
            {
                if (await _conversationService.UserHasAccessAsync(userId, conversationId))
                {
                    await Clients.OthersInGroup($"conversation_{conversationId}")
                        .UserStartedTyping(conversationId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting typing indicator for user {userId} in conversation {conversationId}");
            }
        }

        /// <summary>
        /// Broadcasts that a user stopped typing
        /// </summary>
        public async Task StopTyping(Guid conversationId)
        {
            var userId = CurrentUserId;
            
            try
            {
                if (await _conversationService.UserHasAccessAsync(userId, conversationId))
                {
                    await Clients.OthersInGroup($"conversation_{conversationId}")
                        .UserStoppedTyping(conversationId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping typing indicator for user {userId} in conversation {conversationId}");
            }
        }

        #endregion

        #region Support-Specific Methods

        /// <summary>
        /// Closes a support ticket (support agents only)
        /// </summary>
        public async Task CloseSupportTicket(Guid conversationId, string reason = "Issue resolved")
        {
            var userId = CurrentUserId;
            
            try
            {
                if (!HasClaim("hbg-chat-support"))
                {
                    await _alertService.SendPermissionDeniedAlertAsync(userId, "close support tickets");
                    throw new UnauthorizedAccessException("Only support agents can close tickets");
                }

                // Update conversation status
                var updates = new Dictionary<string, object> { { "IsActive", false } };
                await _conversationService.UpdateConversationAsync(conversationId, updates);

                // Send alerts to participants
                await _alertService.SendSupportTicketClosedAlertAsync(conversationId, userId);
                
                // Create system message
                var systemMessage = Message.CreateSystemMessage(conversationId, $"Support ticket closed: {reason}");
                await _messageService.SaveMessageAsync(systemMessage);
                await Clients.Group($"conversation_{conversationId}").MessageReceived(systemMessage);

                // Notify about read-only mode
                await Clients.Group($"conversation_{conversationId}")
                    .ConversationAccessChanged(conversationId, true, "Support ticket has been closed");

                _logger.LogInformation($"Support ticket {conversationId} closed by agent {userId}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error closing support ticket {conversationId} by user {userId}");
                await _alertService.CreateErrorAlertAsync(userId, "Failed to close support ticket", conversationId);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Filters conversations based on type for different tabs
        /// </summary>
        private IQueryable<Conversation> FilterConversationsByType(
            IQueryable<Conversation> conversations, 
            string conversationType, 
            string userId)
        {
            return conversationType.ToLower() switch
            {
                "contacts" => conversations.Where(c => c.Type == ConversationType.Direct || 
                    (c.Type == ConversationType.Group && c.Title != null && !c.Title.StartsWith("Support:"))),
                
                "support" => HasClaim("hbg-chat-support") 
                    ? conversations.Where(c => c.Title != null && c.Title.StartsWith("Support:"))
                    : conversations.Where(c => c.Title != null && c.Title.StartsWith("Support:") && 
                        c.ParticipantIds.Contains(userId)),
                
                "agent" => conversations.Where(c => c.Title != null && c.Title.StartsWith("AI Agent:")),
                
                _ => conversations
            };
        }

        /// <summary>
        /// Validates permissions for creating conversations of specific types
        /// </summary>
        private async Task ValidateConversationCreationPermissions(string conversationType, List<string> participantIds)
        {
            switch (conversationType.ToLower())
            {
                case "support":
                    // Support conversations can be created by anyone, but are handled specially
                    break;
                
                case "agent":
                    // AI Agent conversations are typically created automatically
                    break;
                
                default: // "contacts"
                    // Regular user conversations - no special validation needed
                    break;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates conversations based on type with appropriate settings
        /// </summary>
        private async Task<Conversation> CreateConversationByType(
            string userId, 
            string title, 
            List<string> participantIds, 
            string conversationType)
        {
            return conversationType.ToLower() switch
            {
                "support" => await _conversationService.CreateGroupConversationAsync(
                    userId, 
                    $"Support: {title ?? "New Ticket"}", 
                    participantIds),
                
                "agent" => await _conversationService.CreateGroupConversationAsync(
                    userId, 
                    $"AI Agent: {title ?? "New Chat"}", 
                    participantIds),
                
                _ => participantIds.Count == 2 && string.IsNullOrEmpty(title)
                    ? await _conversationService.CreateDirectConversationAsync(userId, participantIds.First(p => p != userId))
                    : await _conversationService.CreateGroupConversationAsync(userId, title, participantIds)
            };
        }

        /// <summary>
        /// Creates appropriate system message for new conversations
        /// </summary>
        private async Task<Message> CreateConversationSystemMessage(Conversation conversation, string conversationType)
        {
            var message = conversationType.ToLower() switch
            {
                "support" => $"Support ticket created by {CurrentUserName}",
                "agent" => $"AI Agent conversation started by {CurrentUserName}",
                _ => conversation.Type == ConversationType.Direct ? "Conversation started" : $"{CurrentUserName} created the group"
            };

            return Message.CreateSystemMessage(conversation.ConversationId, message);
        }

        /// <summary>
        /// Adds participants to a SignalR group
        /// </summary>
        private async Task AddParticipantsToGroup(List<string> participantIds, string groupName)
        {
            foreach (var participantId in participantIds)
            {
                var connections = await _connectionService.GetConnectionsAsync(participantId);
                foreach (var connectionId in connections)
                {
                    await Groups.AddToGroupAsync(connectionId, groupName);
                }
            }
        }

        /// <summary>
        /// Handles notifications for conversation updates
        /// </summary>
        private async Task HandleConversationUpdateNotifications(Guid conversationId, Dictionary<string, object> updates)
        {
            foreach (var update in updates)
            {
                switch (update.Key.ToLower())
                {
                    case "title":
                        var systemMessage = Message.CreateSystemMessage(
                            conversationId,
                            $"{CurrentUserName} changed the group title to \"{update.Value}\"");
                        
                        await _messageService.SaveMessageAsync(systemMessage);
                        await Clients.Group($"conversation_{conversationId}").MessageReceived(systemMessage);
                        break;
                }
            }
        }

        /// <summary>
        /// Marks loaded messages as read for the current user
        /// </summary>
        private async Task MarkLoadedMessagesAsRead(LoadResult loadResult, string userId)
        {
            try
            {
                var loadedMessages = loadResult.data as IEnumerable<Message>;
                if (loadedMessages != null)
                {
                    foreach (var message in loadedMessages.Where(m => !m.ReadByUserIds.Contains(userId)))
                    {
                        await MarkMessageAsRead(message.MessageId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error marking messages as read for user {userId}");
            }
        }

        #endregion
    }
}