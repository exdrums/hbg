using API.Contacts.Application.Interfaces;
using API.Contacts.Application.Services;
using API.Contacts.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Examples
{
    /// <summary>
    /// Sample client implementation for the chat functionality
    /// </summary>
    public class ChatClient
    {
        // Sample user IDs for testing
        private const string USER_ID = "user123";
        private const string OTHER_USER_ID = "user456";

        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly ILogger<ChatClient> _logger;
        private HubConnection _hubConnection;

        /// <summary>
        /// Main method to demonstrate the chat client
        /// </summary>
        public static async Task Main(string[] args)
        {
            // Set up services
            var services = new ServiceCollection();
            services.AddChatServices();
            services.AddLogging(builder => builder.AddConsole());

            var serviceProvider = services.BuildServiceProvider();
            var chatClient = ActivatorUtilities.CreateInstance<ChatClient>(serviceProvider);

            // Run the client
            await chatClient.RunAsync();
        }

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ChatClient(
            IChatService chatService,
            IUserService userService,
            ILogger<ChatClient> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Runs the chat client
        /// </summary>
        public async Task RunAsync()
        {
            try
            {
                // Connect to the SignalR hub
                await ConnectToHubAsync();

                // Create or get an existing user
                var user = await _userService.GetOrCreateUserFromOidcAsync($"sub:{USER_ID}", "Test User");
                Console.WriteLine($"Logged in as: {user.Name} (ID: {user.Id})");

                // Display user's conversations
                await DisplayConversationsAsync(user.Id);

                // Create a new conversation
                var conversation = await CreateNewConversationAsync(user.Id);
                Console.WriteLine($"Created conversation: {conversation.Title} (ID: {conversation.Id})");

                // Send and receive messages
                await SendAndReceiveMessagesAsync(conversation.Id, user.Id);

                // Disconnect from the hub
                await _hubConnection.StopAsync();
                Console.WriteLine("Disconnected from chat hub");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running chat client");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Connects to the SignalR hub
        /// </summary>
        private async Task ConnectToHubAsync()
        {
            Console.WriteLine("Connecting to chat hub...");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/chathub")
                .WithAutomaticReconnect()
                .Build();

            // Set up event handlers
            _hubConnection.On<string, object>("ReceiveMessage", (conversationId, message) =>
            {
                Console.WriteLine($"Received message in conversation {conversationId}: {message}");
            });

            _hubConnection.On<string, object[]>("UsersTyping", (conversationId, users) =>
            {
                Console.WriteLine($"Users typing in conversation {conversationId}: {string.Join(", ", users)}");
            });

            _hubConnection.On<object>("AlertsChanged", alerts =>
            {
                Console.WriteLine($"Alerts changed: {alerts}");
            });

            // Start the connection
            await _hubConnection.StartAsync();
            Console.WriteLine("Connected to chat hub");
        }

        /// <summary>
        /// Displays the user's conversations
        /// </summary>
        private async Task DisplayConversationsAsync(string userId)
        {
            Console.WriteLine("Fetching conversations...");
            var conversations = await _chatService.GetUserConversationsAsync(userId);

            if (!conversations.Any())
            {
                Console.WriteLine("No conversations found");
                return;
            }

            Console.WriteLine("Your conversations:");
            foreach (var conversation in conversations)
            {
                var participantNames = string.Join(", ", conversation.Participants.Select(p => p.Name));
                Console.WriteLine($"- {conversation.Title} ({participantNames})");
            }
        }

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        private async Task<API.Contacts.Application.Dtos.ConversationDto> CreateNewConversationAsync(string userId)
        {
            Console.WriteLine("Creating a new conversation...");

            // Create a conversation with another user
            var conversation = await _chatService.CreateConversationAsync(
                userId,
                new[] { OTHER_USER_ID },
                "Test Conversation"
            );

            return conversation;
        }

        /// <summary>
        /// Sends and receives messages in a conversation
        /// </summary>
        private async Task SendAndReceiveMessagesAsync(string conversationId, string userId)
        {
            Console.WriteLine("Sending messages...");

            // Send a message
            await _chatService.SendMessageAsync(conversationId, userId, "Hello, this is a test message!");
            Console.WriteLine("Sent a message");

            // Simulate user typing
            await _chatService.UserStartedTypingAsync(conversationId, userId);
            Console.WriteLine("User started typing");

            await Task.Delay(2000); // Simulate typing for 2 seconds

            await _chatService.UserStoppedTypingAsync(conversationId, userId);
            Console.WriteLine("User stopped typing");

            // Send another message
            await _chatService.SendMessageAsync(conversationId, userId, "This is another test message");
            Console.WriteLine("Sent another message");

            // Mark conversation as read
            await _chatService.MarkConversationAsReadAsync(conversationId, userId);
            Console.WriteLine("Marked conversation as read");

            // Get messages
            var messages = await _chatService.GetConversationMessagesAsync(conversationId, userId, 10);
            Console.WriteLine($"Retrieved {messages.Count()} messages");
        }
    }
}
