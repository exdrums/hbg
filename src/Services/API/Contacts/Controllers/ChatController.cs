using API.Contacts.Application.Dtos;
using API.Contacts.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IAlertService _alertService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatService chatService,
            IUserService userService,
            IAlertService alertService,
            ILogger<ChatController> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all conversations for the current user
        /// </summary>
        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
        {
            try
            {
                var userId = GetUserId();
                var conversations = await _chatService.GetUserConversationsAsync(userId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                return StatusCode(500, "An error occurred while retrieving conversations");
            }
        }

        /// <summary>
        /// Gets a specific conversation by ID
        /// </summary>
        [HttpGet("conversations/{conversationId}")]
        public async Task<ActionResult<ConversationDto>> GetConversation(string conversationId)
        {
            try
            {
                var userId = GetUserId();
                var conversation = await _chatService.GetConversationByIdAsync(conversationId, userId);
                return Ok(conversation);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation {ConversationId}", conversationId);
                return StatusCode(500, "An error occurred while retrieving the conversation");
            }
        }

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        [HttpPost("conversations")]
        public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationRequest request)
        {
            try
            {
                var userId = GetUserId();
                var conversation = await _chatService.CreateConversationAsync(userId, request.ParticipantIds, request.Title);
                return CreatedAtAction(nameof(GetConversation), new { conversationId = conversation.Id }, conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return StatusCode(500, "An error occurred while creating the conversation");
            }
        }

        /// <summary>
        /// Creates a new AI assistant conversation
        /// </summary>
        [HttpPost("conversations/ai")]
        public async Task<ActionResult<ConversationDto>> CreateAiAssistantConversation([FromBody] CreateAiConversationRequest request)
        {
            try
            {
                var userId = GetUserId();
                var conversation = await _chatService.CreateAiAssistantConversationAsync(userId, request.Title);
                return CreatedAtAction(nameof(GetConversation), new { conversationId = conversation.Id }, conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AI assistant conversation");
                return StatusCode(500, "An error occurred while creating the AI assistant conversation");
            }
        }

        /// <summary>
        /// Gets messages from a conversation
        /// </summary>
        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(
            string conversationId,
            [FromQuery] int limit = 50,
            [FromQuery] DateTime? before = null)
        {
            try
            {
                var userId = GetUserId();
                var messages = await _chatService.GetConversationMessagesAsync(conversationId, userId, limit, before);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", conversationId);
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }

        /// <summary>
        /// Sends a message to a conversation
        /// </summary>
        [HttpPost("conversations/{conversationId}/messages")]
        public async Task<ActionResult<MessageDto>> SendMessage(
            string conversationId,
            [FromBody] SendMessageRequest request)
        {
            try
            {
                var userId = GetUserId();
                var message = await _chatService.SendMessageAsync(conversationId, userId, request.Text, request.ParentMessageId);
                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to conversation {ConversationId}", conversationId);
                return StatusCode(500, "An error occurred while sending the message");
            }
        }

        /// <summary>
        /// Sends a message to an AI assistant conversation
        /// </summary>
        [HttpPost("conversations/{conversationId}/ai/messages")]
        public async Task<ActionResult<MessageDto>> SendMessageToAi(
            string conversationId,
            [FromBody] SendMessageRequest request)
        {
            try
            {
                var userId = GetUserId();
                var message = await _chatService.SendMessageToAiAsync(conversationId, userId, request.Text);
                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to AI in conversation {ConversationId}", conversationId);
                return StatusCode(500, "An error occurred while sending the message to AI");
            }
        }

        /// <summary>
        /// Regenerates an AI assistant response
        /// </summary>
        [HttpPost("conversations/{conversationId}/messages/{messageId}/regenerate")]
        public async Task<ActionResult<MessageDto>> RegenerateAiResponse(string conversationId, string messageId)
        {
            try
            {
                var userId = GetUserId();
                var message = await _chatService.RegenerateAiResponseAsync(conversationId, messageId, userId);
                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating AI response for message {MessageId}", messageId);
                return StatusCode(500, "An error occurred while regenerating the AI response");
            }
        }

        /// <summary>
        /// Edits a message
        /// </summary>
        [HttpPut("conversations/{conversationId}/messages/{messageId}")]
        public async Task<ActionResult<MessageDto>> EditMessage(
            string conversationId,
            string messageId,
            [FromBody] EditMessageRequest request)
        {
            try
            {
                var userId = GetUserId();
                var message = await _chatService.EditMessageAsync(messageId, userId, request.Text);
                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message {MessageId}", messageId);
                return StatusCode(500, "An error occurred while editing the message");
            }
        }

        /// <summary>
        /// Marks a conversation as read
        /// </summary>
        [HttpPost("conversations/{conversationId}/read")]
        public async Task<ActionResult> MarkAsRead(string conversationId)
        {
            try
            {
                var userId = GetUserId();
                await _chatService.MarkConversationAsReadAsync(conversationId, userId);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation {ConversationId} as read", conversationId);
                return StatusCode(500, "An error occurred while marking the conversation as read");
            }
        }

        /// <summary>
        /// Archives a conversation
        /// </summary>
        [HttpPost("conversations/{conversationId}/archive")]
        public async Task<ActionResult> ArchiveConversation(string conversationId)
        {
            try
            {
                var userId = GetUserId();
                await _chatService.ArchiveConversationAsync(conversationId, userId);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving conversation {ConversationId}", conversationId);
                return StatusCode(500, "An error occurred while archiving the conversation");
            }
        }

        /// <summary>
        /// Unarchives a conversation
        /// </summary>
        [HttpPost("conversations/{conversationId}/unarchive")]
        public async Task<ActionResult> UnarchiveConversation(string conversationId)
        {
            try
            {
                var userId = GetUserId();
                await _chatService.UnarchiveConversationAsync(conversationId, userId);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unarchiving conversation {ConversationId}", conversationId);
                return StatusCode(500, "An error occurred while unarchiving the conversation");
            }
        }

        /// <summary>
        /// Gets all active alerts for the current user
        /// </summary>
        [HttpGet("alerts")]
        public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlerts()
        {
            try
            {
                var userId = GetUserId();
                var alerts = await _alertService.GetActiveAlertsForUserAsync(userId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts");
                return StatusCode(500, "An error occurred while retrieving alerts");
            }
        }

        /// <summary>
        /// Marks all alerts as read
        /// </summary>
        [HttpPost("alerts/read")]
        public async Task<ActionResult> MarkAllAlertsAsRead()
        {
            try
            {
                var userId = GetUserId();
                await _alertService.MarkAllAsReadAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all alerts as read");
                return StatusCode(500, "An error occurred while marking alerts as read");
            }
        }

        /// <summary>
        /// Dismisses all alerts
        /// </summary>
        [HttpPost("alerts/dismiss")]
        public async Task<ActionResult> DismissAllAlerts()
        {
            try
            {
                var userId = GetUserId();
                await _alertService.DismissAllAlertsAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing all alerts");
                return StatusCode(500, "An error occurred while dismissing alerts");
            }
        }

        /// <summary>
        /// Dismisses a specific alert
        /// </summary>
        [HttpPost("alerts/{alertId}/dismiss")]
        public async Task<ActionResult> DismissAlert(string alertId)
        {
            try
            {
                var userId = GetUserId();
                await _alertService.DismissAlertAsync(alertId, userId);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing alert {AlertId}", alertId);
                return StatusCode(500, "An error occurred while dismissing the alert");
            }
        }

        /// <summary>
        /// Gets the current user
        /// </summary>
        [HttpGet("user")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userId = GetUserId();
                var user = await _userService.GetByIdAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, "An error occurred while retrieving user information");
            }
        }

        /// <summary>
        /// Updates the current user's profile
        /// </summary>
        [HttpPut("user")]
        public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetUserId();
                var user = await _userService.UpdateProfileAsync(userId, request.Name, request.AvatarUrl, request.AvatarAlt);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }

        /// <summary>
        /// Helper method to get the current user ID from the claims
        /// </summary>
        private string GetUserId()
        {
            var subject = User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(subject))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            return subject;
        }
    }

    // Request DTOs
    public class CreateConversationRequest
    {
        [Required]
        public IEnumerable<string> ParticipantIds { get; set; }

        public string Title { get; set; }
    }

    public class CreateAiConversationRequest
    {
        public string Title { get; set; } = "AI Assistant";
    }

    public class SendMessageRequest
    {
        [Required]
        public string Text { get; set; }

        public string ParentMessageId { get; set; }
    }

    public class EditMessageRequest
    {
        [Required]
        public string Text { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required]
        public string Name { get; set; }

        public string AvatarUrl { get; set; }

        public string AvatarAlt { get; set; }
    }
}
