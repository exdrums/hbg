using API.Contacts.Application.Dtos;
using API.Contacts.Application.Interfaces;
using API.Contacts.Application.Services;
using API.Contacts.Domain.Models;
using API.Contacts.Domain.Repositories;
using API.Contacts.Infrastructure.Repositories.InMemory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace API.Contacts.Tests
{
    /// <summary>
    /// Unit tests for the ChatService
    /// </summary>
    public class ChatServiceTests
    {
        private readonly Mock<IConversationRepository> _mockConversationRepository;
        private readonly Mock<IMessageRepository> _mockMessageRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly Mock<IRealtimeNotificationService> _mockNotificationService;
        private readonly Mock<IReadReceiptService> _mockReadReceiptService;
        private readonly Mock<ITypingService> _mockTypingService;
        private readonly Mock<IAiAssistantService> _mockAiAssistantService;
        private readonly Mock<ILogger<ChatService>> _mockLogger;
        private readonly ChatService _chatService;

        /// <summary>
        /// Constructor to set up the test environment
        /// </summary>
        public ChatServiceTests()
        {
            _mockConversationRepository = new Mock<IConversationRepository>();
            _mockMessageRepository = new Mock<IMessageRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockNotificationService = new Mock<IRealtimeNotificationService>();
            _mockReadReceiptService = new Mock<IReadReceiptService>();
            _mockTypingService = new Mock<ITypingService>();
            _mockAiAssistantService = new Mock<IAiAssistantService>();
            _mockLogger = new Mock<ILogger<ChatService>>();

            _chatService = new ChatService(
                _mockConversationRepository.Object,
                _mockMessageRepository.Object,
                _mockUserRepository.Object,
                _mockAuthService.Object,
                _mockNotificationService.Object,
                _mockReadReceiptService.Object,
                _mockTypingService.Object,
                _mockAiAssistantService.Object,
                _mockLogger.Object
            );
        }

        /// <summary>
        /// Test creating a new conversation
        /// </summary>
        [Fact]
        public async Task CreateConversationAsync_ShouldCreateNewConversation()
        {
            // Arrange
            var creatorId = "user1";
            var participantIds = new[] { "user2", "user3" };
            var title = "Test Group";
            var conversationId = "conversation1";

            var conversation = new Conversation(conversationId, title, ConversationType.Group);
            conversation.AddParticipant(creatorId, ParticipantRole.Admin);
            foreach (var participantId in participantIds)
            {
                conversation.AddParticipant(participantId);
            }

            var creator = new User(creatorId, "oidc1", "Creator");
            var participants = new List<User>
            {
                creator,
                new User("user2", "oidc2", "User 2"),
                new User("user3", "oidc3", "User 3")
            };

            _mockConversationRepository.Setup(r => r.AddAsync(It.IsAny<Conversation>()))
                .ReturnsAsync(true);
            _mockConversationRepository.Setup(r => r.GetByIdAsync(conversationId))
                .ReturnsAsync(conversation);
            _mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .Returns<string>(id => Task.FromResult(participants.FirstOrDefault(p => p.Id == id)));
            _mockReadReceiptService.Setup(r => r.GetLastReadTimestampAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(DateTime.MinValue);
            _mockMessageRepository.Setup(r => r.GetUnreadCountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(0);
            _mockMessageRepository.Setup(r => r.GetByConversationIdAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Message>());

            // Act
            var result = await _chatService.CreateConversationAsync(creatorId, participantIds, title);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(title, result.Title);
            Assert.Equal(ConversationType.Group, result.Type);
            Assert.Equal(3, result.Participants.Count);
        }

        /// <summary>
        /// Test sending a message
        /// </summary>
        [Fact]
        public async Task SendMessageAsync_ShouldSendMessage()
        {
            // Arrange
            var userId = "user1";
            var conversationId = "conversation1";
            var text = "Hello, world!";
            var messageId = "message1";

            var user = new User(userId, "oidc1", "Test User");
            var conversation = new Conversation(conversationId, "Test Conversation", ConversationType.Group);
            conversation.AddParticipant(userId);

            var message = new Message(messageId, conversationId, userId, text, DateTime.UtcNow);

            _mockAuthService.Setup(a => a.IsUserAuthorizedForConversationAsync(userId, conversationId))
                .ReturnsAsync(true);
            _mockConversationRepository.Setup(r => r.GetByIdAsync(conversationId))
                .ReturnsAsync(conversation);
            _mockMessageRepository.Setup(r => r.AddAsync(It.IsAny<Message>()))
                .ReturnsAsync(true);
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _mockMessageRepository.Setup(r => r.GetByIdAsync(messageId))
                .ReturnsAsync(message);

            // Act
            var result = await _chatService.SendMessageAsync(conversationId, userId, text);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(text, result.Text);
            Assert.Equal(userId, result.Author.Id);
            _mockNotificationService.Verify(n => n.NotifyMessageReceived(conversationId, It.IsAny<MessageDto>()), Times.Once);
        }

        /// <summary>
        /// Test marking messages as read
        /// </summary>
        [Fact]
        public async Task MarkConversationAsReadAsync_ShouldMarkMessagesAsRead()
        {
            // Arrange
            var userId = "user1";
            var conversationId = "conversation1";

            _mockAuthService.Setup(a => a.IsUserAuthorizedForConversationAsync(userId, conversationId))
                .ReturnsAsync(true);
            _mockReadReceiptService.Setup(r => r.MarkMessagesAsReadAsync(conversationId, userId, It.IsAny<DateTime>()))
                .ReturnsAsync(5);
            _mockReadReceiptService.Setup(r => r.GetReadReceiptsForConversationAsync(conversationId))
                .ReturnsAsync(new Dictionary<string, DateTime> { { userId, DateTime.UtcNow } });

            // Act
            await _chatService.MarkConversationAsReadAsync(conversationId, userId);

            // Assert
            _mockReadReceiptService.Verify(r => r.MarkMessagesAsReadAsync(conversationId, userId, It.IsAny<DateTime>()), Times.Once);
            _mockNotificationService.Verify(n => n.NotifyReadReceiptsUpdated(conversationId, It.IsAny<IDictionary<string, DateTime>>()), Times.Once);
        }

        /// <summary>
        /// Test creating an AI assistant conversation
        /// </summary>
        [Fact]
        public async Task CreateAiAssistantConversationAsync_ShouldCreateAiConversation()
        {
            // Arrange
            var userId = "user1";
            var aiUserId = "ai-assistant";
            var title = "AI Assistant";
            var conversationId = "conversation1";

            var user = new User(userId, "oidc1", "Test User");
            var aiUser = new User(aiUserId, "ai-assistant", "AI Assistant");

            var conversation = new Conversation(conversationId, title, ConversationType.AiAssistant);
            conversation.AddParticipant(userId);
            conversation.AddParticipant(aiUserId, ParticipantRole.AiAssistant);

            _mockAiAssistantService.Setup(a => a.CreateAiAssistantUserAsync())
                .ReturnsAsync(aiUser);
            _mockConversationRepository.Setup(r => r.AddAsync(It.IsAny<Conversation>()))
                .ReturnsAsync(true);
            _mockConversationRepository.Setup(r => r.GetByIdAsync(conversationId))
                .ReturnsAsync(conversation);
            _mockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .Returns<string>(id => Task.FromResult(id == userId ? user : (id == aiUserId ? aiUser : null)));
            _mockMessageRepository.Setup(r => r.AddAsync(It.IsAny<Message>()))
                .ReturnsAsync(true);
            _mockReadReceiptService.Setup(r => r.GetLastReadTimestampAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(DateTime.MinValue);
            _mockMessageRepository.Setup(r => r.GetUnreadCountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(0);

            // Act
            var result = await _chatService.CreateAiAssistantConversationAsync(userId, title);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(title, result.Title);
            Assert.Equal(ConversationType.AiAssistant, result.Type);
            Assert.Equal(2, result.Participants.Count);

            var aiParticipant = result.Participants.FirstOrDefault(p => p.Id == aiUserId);
            Assert.NotNull(aiParticipant);
            Assert.Equal("AI Assistant", aiParticipant.Name);
        }
    }
}
