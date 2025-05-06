# Chat Functionality

This document provides an overview of the refactored chat functionality for the HBG application.

## Architecture

The chat system has been refactored to follow Domain-Driven Design (DDD) principles with a clean architecture approach. The main components are:

### Domain Layer

- **Domain Models**: Represents the core business entities such as `User`, `Conversation`, `Message`, and `Alert`.
- **Repository Interfaces**: Defines contracts for data access operations.

### Application Layer

- **Application Services**: Implements business logic and coordinates the domain layer.
- **DTOs**: Data Transfer Objects for communication with the presentation layer.

### Infrastructure Layer

- **Repository Implementations**: Concrete implementations of the repository interfaces.
- **SignalR Hub**: Real-time communication infrastructure.
- **External Services**: Implementation of external service integrations.

### Presentation Layer

- **REST API Controllers**: HTTP endpoints for client applications.
- **SignalR Hub**: Real-time messaging endpoints.

## Key Features

- **Real-time Messaging**: Users can send and receive messages in real time.
- **Typing Indicators**: Shows when users are typing.
- **Read Receipts**: Tracks when messages are read by recipients.
- **Alerts and Notifications**: Notifies users of new messages and other events.
- **AI Assistant**: Integration with an AI service for conversational assistance.
- **File Attachments**: Support for sharing files in conversations.

## Implementation Details

### Core Services

1. **ChatService**: Main service for chat operations.
2. **UserService**: Manages user-related operations.
3. **MessageService**: Handles message creation, editing, and retrieval.
4. **AlertService**: Manages alerts and notifications.
5. **ReadReceiptService**: Tracks message read status.
6. **TypingService**: Manages typing indicators.
7. **AuthenticationService**: Handles authorization checks.
8. **AiAssistantService**: Integrates with AI services for conversational assistance.

### Real-time Communication

The chat system uses SignalR for real-time communication between clients and the server. The main components are:

1. **ChatHub**: SignalR hub for chat operations.
2. **ConnectionManager**: Manages client connections.
3. **NotificationService**: Sends real-time notifications to clients.

### Data Persistence

The current implementation uses in-memory repositories for simplicity, but these can be replaced with database implementations. The repositories are:

1. **UserRepository**: Stores user data.
2. **ConversationRepository**: Stores conversation data.
3. **MessageRepository**: Stores message data.
4. **AlertRepository**: Stores alert data.

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- An OpenID Connect identity provider (e.g., Identity Server)

### Configuration

1. Configure the application settings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your database connection string"
  },
  "AiAssistant": {
    "Endpoint": "Your AI service endpoint",
    "ApiKey": "Your AI service API key",
    "DeploymentName": "Your deployment name",
    "MaxRequestsPerHour": 10,
    "MaxTokens": 2000,
    "Temperature": 0.7,
    "SystemPrompt": "You are a helpful assistant."
  },
  "FileStorage": {
    "BasePath": "Path to file storage directory"
  }
}
```

2. Configure authentication in the `ConfigureServices` method in `Services.cs`.

### Starting the Server

1. Navigate to the project directory.
2. Run the application:

```bash
dotnet run
```

### Client Integration

Clients can interact with the chat system through:

1. **REST API**: HTTP endpoints for CRUD operations.
2. **SignalR Hub**: Real-time communication for chat operations.

#### SignalR Hub Methods

- `JoinConversation(string conversationId)`: Joins a conversation.
- `LeaveConversation(string conversationId)`: Leaves a conversation.
- `LoadConversations()`: Gets all conversations for the current user.
- `SendMessage(string conversationId, string text, string parentMessageId)`: Sends a message.
- `UserStartedTyping(string conversationId)`: Indicates user started typing.
- `UserStoppedTyping(string conversationId)`: Indicates user stopped typing.
- `MarkAsRead(string conversationId)`: Marks messages as read.

#### SignalR Hub Events

- `ReceiveMessage(string conversationId, MessageDto message)`: Notifies of new messages.
- `UsersTyping(string conversationId, UserDto[] users)`: Notifies of users typing.
- `ReadReceiptsUpdated(string conversationId, Dictionary<string, DateTime> readReceipts)`: Notifies of read receipt updates.
- `AlertsChanged(AlertDto[] alerts)`: Notifies of alert changes.

## Example Usage

See the `Examples/ChatClient.cs` file for a complete example of how to use the chat functionality.

## Future Improvements

1. **Database Persistence**: Replace in-memory repositories with database implementations.
2. **Authentication**: Enhance authentication and authorization.
3. **File Uploads**: Improve file attachment handling.
4. **Message Encryption**: Add end-to-end encryption for messages.
5. **Push Notifications**: Add support for mobile push notifications.
6. **Message Search**: Implement full-text search for messages.
7. **Group Management**: Enhanced group conversation management.
8. **Message Reactions**: Add support for message reactions.
9. **Message Formatting**: Support for rich text and markdown in messages.
10. **Threading**: Improved support for message threads and replies.
