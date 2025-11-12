using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using API.Constructor.Models.Enums;
using API.Constructor.Services;

namespace API.Constructor.WebSocket
{
    [Authorize]
    public class ConstructorHub : Hub
    {
        private readonly IConfigurationService _configService;
        private readonly IProjectService _projectService;
        private readonly ILogger<ConstructorHub> _logger;

        public ConstructorHub(
            IConfigurationService configService,
            IProjectService projectService,
            ILogger<ConstructorHub> logger)
        {
            _configService = configService;
            _projectService = projectService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            _logger.LogInformation("User {UserId} connected to Constructor Hub. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            _logger.LogInformation("User {UserId} disconnected from Constructor Hub. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Send a chat message to modify jewelry configuration
        /// </summary>
        public async Task SendChatMessage(Guid projectId, string message)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            _logger.LogInformation("User {UserId} sent chat message for project {ProjectId}: {Message}",
                userId, projectId, message);

            try
            {
                // Get the project to verify access
                var project = await _projectService.GetProjectAsync(projectId, userId);

                if (project == null)
                {
                    await Clients.Caller.SendAsync("ReceiveError", new
                    {
                        Message = "Project not found or access denied"
                    });
                    return;
                }

                // TODO: Process chat message with AI to extract configuration changes
                // For now, we'll send a response indicating the message was received
                await Clients.Caller.SendAsync("ReceiveChatResponse", new
                {
                    Message = "Message received. AI processing is not yet implemented. Please use the configuration form to generate images.",
                    UserMessage = message,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Chat message processed for project {ProjectId}", projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                await Clients.Caller.SendAsync("ReceiveError", new
                {
                    Message = "An error occurred while processing your message"
                });
            }
        }

        /// <summary>
        /// Regenerate an image from an existing configuration
        /// </summary>
        public async Task RegenerateImage(Guid configurationId)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            _logger.LogInformation("User {UserId} requested image regeneration for configuration {ConfigurationId}",
                userId, configurationId);

            try
            {
                var configuration = await _configService.GetConfigurationAsync(configurationId);

                if (configuration == null || configuration.Project.UserId != userId)
                {
                    await Clients.Caller.SendAsync("ReceiveError", new
                    {
                        Message = "Configuration not found or access denied"
                    });
                    return;
                }

                // Generate image
                var image = await _configService.GenerateAndSaveImageAsync(
                    configurationId,
                    "1:1",
                    GenerationSource.Chat
                );

                // Send update to user
                await Clients.User(userId).SendAsync("ReceiveImageUpdate", new
                {
                    ImageId = image.ImageId,
                    ConfigurationId = image.ConfigurationId,
                    ImageUrl = image.FileServiceUrl,
                    ThumbnailUrl = image.ThumbnailUrl,
                    GeneratedAt = image.GeneratedAt,
                    Message = "Image generated successfully"
                });

                _logger.LogInformation("Image regenerated successfully: {ImageId}", image.ImageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating image");
                await Clients.Caller.SendAsync("ReceiveError", new
                {
                    Message = $"An error occurred while generating the image: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Notify when image generation starts
        /// </summary>
        public async Task NotifyGenerationStarted(Guid projectId)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            await Clients.User(userId).SendAsync("GenerationStarted", new
            {
                ProjectId = projectId,
                Message = "Image generation started"
            });
        }
    }
}
