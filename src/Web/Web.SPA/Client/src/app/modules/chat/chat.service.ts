import { Injectable } from '@angular/core';
import { Observable, firstValueFrom, map } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { User } from './models/user';
import { ChatSignalRConnection } from './chat-connection.service';
import { ChatMessageStore } from './messages.data-store';
import { ChatConversationStore } from './conversations.data-store';
import { Conversation } from './models/conversation';
import { ConversationType } from './models/conversation-type';
import { Message } from './models/message';
import { Alert } from './models/alert';

/**
 * Main service for chat functionality
 * This service coordinates between the connection, stores, and UI components
 */
@Injectable()
export class ChatService {
  // Current user information
  private currentUser: User;

  constructor(
    private authService: AuthService,
    private chatConnection: ChatSignalRConnection,
    private messageStore: ChatMessageStore,
    private conversationStore: ChatConversationStore
  ) {
    // Initialize the current user when authentication is available
    this.authService.userProfile$.subscribe(userProfile => {
      if (userProfile) {
        this.currentUser = {
          id: userProfile.sub,
          name: userProfile.name,
          avatarUrl: userProfile.picture
        };
        this.messageStore.setCurrentUser(this.currentUser);
      }
    });
  }

  /**
   * Get the current user
   */
  public getCurrentUser(): User {
    return this.currentUser;
  }

  /**
   * Get all conversations for the current user as an observable
   */
  public getConversations(): Observable<Conversation[]> {
    return this.chatConnection.conversations$;
  }

  /**
   * Get a specific conversation by ID
   */
  public async getConversation(conversationId: string): Promise<Conversation | undefined> {
    const conversations = await firstValueFrom(this.getConversations());
    return conversations.find(c => c.id === conversationId);
  }
  
  /**
   * Load all conversations for the current user
   * This triggers an immediate server refresh
   */
  public async loadAllConversations(): Promise<Conversation[]> {
    return await this.chatConnection.loadConversations();
  }

  /**
   * Create a new data source for a conversation's messages
   */
  public createMessageDataSource(conversationId: string, conversationType: ConversationType) {
    return this.messageStore.createDataSource(conversationId, conversationType);
  }
  
  /**
   * Create a new data source for conversations
   */
  public createConversationDataSource() {
    return this.conversationStore.createDataSource();
  }

  /**
   * Create a new conversation with specified participants
   */
  public async createConversation(participantIds: string[], title?: string): Promise<Conversation> {
    return await this.conversationStore.createConversation(participantIds, title);
  }
  
  /**
   * Create a new AI assistant conversation
   */
  public async createAiAssistantConversation(title: string = 'AI Assistant'): Promise<Conversation> {
    return await this.conversationStore.createAiAssistantConversation(title);
  }

  /**
   * Request AI to regenerate a response
   */
  public async regenerateAiResponse(conversationId: string, messageId: string): Promise<void> {
    return await this.messageStore.regenerateAiResponse(conversationId, messageId);
  }

  /**
   * Get messages for a conversation
   */
  public getMessages(conversationId: string): Observable<Message[]> {
    return this.chatConnection.getMessagesObservable(conversationId);
  }

  /**
   * Get users who are currently typing in a conversation
   */
  public getTypingUsers(conversationId: string): Observable<User[]> {
    return this.chatConnection.getTypingUsersObservable(conversationId);
  }

  /**
   * Send a message in a conversation
   */
  public async sendMessage(conversationId: string, text: string): Promise<Message> {
    const conversation = await this.getConversation(conversationId);
    
    if (!conversation) {
      throw new Error(`Conversation with ID ${conversationId} not found`);
    }
    
    if (conversation.type === ConversationType.AiAssistant) {
      return await this.chatConnection.sendMessageToAi(conversationId, text);
    } else {
      return await this.chatConnection.sendMessage(conversationId, text);
    }
  }

  /**
   * Notify that the user has started typing
   */
  public async notifyTypingStarted(conversationId: string): Promise<void> {
    await this.chatConnection.userStartedTyping(conversationId);
  }

  /**
   * Notify that the user has stopped typing
   */
  public async notifyTypingStopped(conversationId: string): Promise<void> {
    await this.chatConnection.userStoppedTyping(conversationId);
  }

  /**
   * Get all active alerts for the current user
   */
  public getAlerts(): Observable<Alert[]> {
    return this.chatConnection.alerts$;
  }

  /**
   * Mark all messages in a conversation as read
   */
  public async markAsRead(conversationId: string): Promise<void> {
    await this.chatConnection.markAsRead(conversationId);
  }

  /**
   * Check connection status
   */
  public isConnected(): Observable<boolean> {
    return this.chatConnection.isConnected$;
  }
}