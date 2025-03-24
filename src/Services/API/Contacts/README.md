# Chat Microservice

This microservice provides real-time chat functionality for the application. It uses SignalR for real-time communication and integrates with the Angular client via DevExtreme UI components.

## Features

- Real-time messaging between users
- Group conversations
- AI assistant integration
- Typing indicators
- Read receipts
- File attachments
- Message history
- OIDC authentication integration

## Architecture

The chat microservice follows a clean architecture approach:

- **Domain Layer**: Entity models and domain logic
- **Application Layer**: Services that orchestrate the domain operations
- **Infrastructure Layer**: External integrations and technical implementations
- **API Layer**: REST controllers and SignalR hub

## Configuration

The service is configured through the `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=chat;Username=postgres;Password=postgres"
  },
  "AppSettings": {
    "HBGIDENTITY": "http://localhost:5000",
    "AUDIENCE": "api_contacts",
    "HBGSPA": "http://localhost:4200",
    "HBGSPADEV": "http://localhost:4200"
  },
  "FileStorage": {
    "BasePath": "./ChatFiles",
    "MaxFileSizeMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv", ".zip"]
  },
  "AiAssistant": {
    "Endpoint": "https://public-api.devexpress.com/demo-openai",
    "ApiKey": "DEMO",
    "DeploymentName": "gpt-4o-mini",
    "MaxTokens": 1000,
    "Temperature": 0.7,
    "ApiVersion": "2024-02-01",
    "RequestLimitPerMinute": 5,
    "RequestLimitCooldownMinutes": 1
  }
}
```

## API Endpoints

### REST API

- `POST /api/files/upload/{conversationId}` - Upload a file to a conversation
- `GET /api/files/{fileId}` - Download a file

### SignalR Hub Methods

#### Connection Management
- `JoinConversation(string conversationId)` - Join a specific conversation
- `LeaveConversation(string conversationId)` - Leave a conversation

#### Conversation Operations
- `LoadConversations()` - Get all conversations for the current user
- `CreateConversation(string[] participantIds, string title)` - Create a new conversation
- `CreateAiAssistantConversation(string title)` - Create a new AI assistant conversation

#### Messaging
- `SendMessage(string conversationId, string text, string parentMessageId)` - Send a message
- `SendMessageToAi(string conversationId, string text)` - Send a message to AI assistant
- `RegenerateAiResponse(string conversationId, string messageId)` - Regenerate an AI response

#### Typing Indicators
- `UserStartedTyping(string conversationId)` - Indicate that user started typing
- `UserStoppedTyping(string conversationId)` - Indicate that user stopped typing

#### Read Receipts
- `MarkAsRead(string conversationId)` - Mark messages as read
- `GetReadReceipts(string conversationId)` - Get read receipts for a conversation

#### File Handling
- `IsFileMessage(string messageText)` - Check if a message contains a file
- `GetFileInfo(string messageText)` - Extract file information from a message

### SignalR Client Events

- `ReceiveMessage` - When a new message is received
- `LoadMessages` - When loading message history
- `UserStartedTyping` - When a user starts typing
- `UserStoppedTyping` - When a user stops typing
- `AlertsChanged` - When user alerts change
- `ReadReceiptsUpdated` - When read receipts change
- `ConversationCreated` - When a new conversation is created
- `ConversationUpdated` - When a conversation is updated
- `ConversationArchived` - When a conversation is archived
- `ConversationRestored` - When a conversation is restored

## Angular Integration

This microservice integrates with an Angular client using:

1. DevExtreme Chat component
2. Custom SignalR data stores (ChatMessageStore, ChatConversationStore)
3. SignalR connection service

To use the chat in an Angular component:

```typescript
import { ChatSignalRService } from '../services/chat-signalr.service';

@Component({...})
export class MyComponent {
  conversation: Conversation;
  messageDataSource: any;

  constructor(private chatService: ChatSignalRService) {}

  ngOnInit() {
    // Get a conversation
    this.chatService.getConversation('conversation-id').then(conversation => {
      this.conversation = conversation;
      
      // Create a data source for messages
      this.messageDataSource = this.chatService.createMessageDataSource(
        conversation.id,
        conversation.type
      );
    });
  }
}
```

## Development

To run the service:

1. Update the connection string in `appsettings.json`
2. Run the service: `dotnet run`
3. The SignalR hub will be available at `/chathub`