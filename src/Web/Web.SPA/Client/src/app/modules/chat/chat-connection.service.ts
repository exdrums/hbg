import { Injectable } from '@angular/core';
import { HttpTransportType, HubConnectionBuilder, JsonHubProtocol, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable, firstValueFrom, filter } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';
import { Alert } from './models/alert';
import { Conversation } from './models/conversation';
import { Message } from './models/message';
import { User } from './models/user';
import { SignalRAction, WsConnection } from '@app/core/services/websocket/ws-connection';

/**
 * This service manages the SignalR connection to the chat hub
 * It extends the base WsConnection class to handle connection lifecycle
 */
@Injectable()
export class ChatSignalRConnection extends WsConnection {
  // Base URL for the chat hub endpoint
  protected hubUrl = `${environment.apiBaseUrl}/chathub`;
  
  // Track connected conversations to properly handle reconnections
  private connectedConversations = new Set<string>();
  
  // Subject for tracking available conversations
  private conversationsSubject = new BehaviorSubject<Conversation[]>([]);
  public conversations$ = this.conversationsSubject.asObservable();
  
  // Subject for tracking alerts
  private alertsSubject = new BehaviorSubject<Alert[]>([]);
  public alerts$ = this.alertsSubject.asObservable();
  
  // Track message listeners per conversation
  private messageListeners = new Map<string, BehaviorSubject<Message[]>>();
  
  // Track users that are typing in each conversation
  private typingUsersMap = new Map<string, BehaviorSubject<User[]>>();

  // Define all SignalR action handlers
  protected actions: SignalRAction[] = [
    { name: 'ReceiveMessage', handler: this.handleReceiveMessage.bind(this) },
    { name: 'LoadMessages', handler: this.handleLoadMessages.bind(this) },
    { name: 'UserStartedTyping', handler: this.handleUserStartedTyping.bind(this) },
    { name: 'UserStoppedTyping', handler: this.handleUserStoppedTyping.bind(this) },
    { name: 'AlertsChanged', handler: this.handleAlertsChanged.bind(this) }
  ];

  constructor(
    protected readonly auth: AuthService
  ) {
    // The connection is activated when authentication state is valid
    super(
      auth,
      auth.isAuthenticated$,
      new JsonHubProtocol()
    );
    
    // Handle reconnection logic when connection status changes
    this.isConnected$.subscribe(connected => {
      if (connected) {
        this.handleReconnection();
      }
    });
  }

  /**
   * Check if connection is allowed - only for authenticated users
   */
  protected canConnect(): boolean {
    return this.auth.isAuthenticated();
  }

  /**
   * Handle reconnection by rejoining all previously connected conversations
   */
  private async handleReconnection(): Promise<void> {
    try {
      // Load all conversations on reconnection
      await this.loadConversations();
      
      // Rejoin all previously connected conversations
      for (const conversationId of this.connectedConversations) {
        await this.joinConversation(conversationId);
      }
    } catch (error) {
      console.error('Error during reconnection:', error);
    }
  }

  /**
   * Load all conversations for the current user
   */
  public async loadConversations(): Promise<Conversation[]> {
    try {
      await this.isConnectedPromise();
      const conversations = await this.connection.invoke('LoadConversations');
      this.conversationsSubject.next(conversations);
      return conversations;
    } catch (error) {
      console.error('Error loading conversations:', error);
      return [];
    }
  }

  /**
   * Join a specific conversation
   */
  public async joinConversation(conversationId: string): Promise<void> {
    try {
      await this.isConnectedPromise();
      await this.connection.invoke('JoinConversation', conversationId);
      this.connectedConversations.add(conversationId);
      
      // Initialize a subject for this conversation if it doesn't exist
      if (!this.messageListeners.has(conversationId)) {
        this.messageListeners.set(conversationId, new BehaviorSubject<Message[]>([]));
      }
      
      // Initialize typing users for this conversation
      if (!this.typingUsersMap.has(conversationId)) {
        this.typingUsersMap.set(conversationId, new BehaviorSubject<User[]>([]));
      }
    } catch (error) {
      console.error(`Error joining conversation ${conversationId}:`, error);
    }
  }

  /**
   * Leave a specific conversation
   */
  public async leaveConversation(conversationId: string): Promise<void> {
    try {
      await this.isConnectedPromise();
      await this.connection.invoke('LeaveConversation', conversationId);
      this.connectedConversations.delete(conversationId);
    } catch (error) {
      console.error(`Error leaving conversation ${conversationId}:`, error);
    }
  }

  /**
   * Get messages observable for a specific conversation
   */
  public getMessagesObservable(conversationId: string): Observable<Message[]> {
    if (!this.messageListeners.has(conversationId)) {
      this.messageListeners.set(conversationId, new BehaviorSubject<Message[]>([]));
    }
    return this.messageListeners.get(conversationId).asObservable();
  }

  /**
   * Get typing users observable for a specific conversation
   */
  public getTypingUsersObservable(conversationId: string): Observable<User[]> {
    if (!this.typingUsersMap.has(conversationId)) {
      this.typingUsersMap.set(conversationId, new BehaviorSubject<User[]>([]));
    }
    return this.typingUsersMap.get(conversationId).asObservable();
  }

  /**
   * Send a message to a conversation
   */
  public async sendMessage(conversationId: string, text: string, parentMessageId?: string): Promise<Message> {
    await this.isConnectedPromise();
    return await this.connection.invoke('SendMessage', conversationId, text, parentMessageId);
  }

  /**
   * Send a message to an AI assistant
   */
  public async sendMessageToAi(conversationId: string, text: string): Promise<Message> {
    await this.isConnectedPromise();
    return await this.connection.invoke('SendMessageToAi', conversationId, text);
  }

  /**
   * Create a new conversation with the specified participants
   */
  public async createConversation(participantIds: string[], title?: string): Promise<Conversation> {
    await this.isConnectedPromise();
    const newConversation = await this.connection.invoke('CreateConversation', participantIds, title);
    // Update the conversations list
    const currentConversations = this.conversationsSubject.getValue();
    this.conversationsSubject.next([...currentConversations, newConversation]);
    return newConversation;
  }

  /**
   * Create a new AI assistant conversation
   */
  public async createAiAssistantConversation(title: string = 'AI Assistant'): Promise<Conversation> {
    await this.isConnectedPromise();
    const newConversation = await this.connection.invoke('CreateAiAssistantConversation', title);
    // Update the conversations list
    const currentConversations = this.conversationsSubject.getValue();
    this.conversationsSubject.next([...currentConversations, newConversation]);
    return newConversation;
  }

  /**
   * Regenerate an AI assistant response
   */
  public async regenerateAiResponse(conversationId: string, messageId: string): Promise<Message> {
    await this.isConnectedPromise();
    return await this.connection.invoke('RegenerateAiResponse', conversationId, messageId);
  }

  /**
   * Notify that user started typing
   */
  public async userStartedTyping(conversationId: string): Promise<void> {
    await this.isConnectedPromise();
    await this.connection.invoke('UserStartedTyping', conversationId);
  }

  /**
   * Notify that user stopped typing
   */
  public async userStoppedTyping(conversationId: string): Promise<void> {
    await this.isConnectedPromise();
    await this.connection.invoke('UserStoppedTyping', conversationId);
  }

  /**
   * Mark all messages in a conversation as read
   */
  public async markAsRead(conversationId: string): Promise<void> {
    await this.isConnectedPromise();
    await this.connection.invoke('MarkAsRead', conversationId);
  }

  // SignalR event handlers

  /**
   * Handle receiving a new message
   */
  private handleReceiveMessage(conversationId: string, message: Message): void {
    if (this.messageListeners.has(conversationId)) {
      const messagesSubject = this.messageListeners.get(conversationId);
      const currentMessages = messagesSubject.getValue();
      
      // If the message already exists (being regenerated), replace it
      const existingIndex = currentMessages.findIndex(m => m.id === message.id);
      
      if (existingIndex >= 0) {
        const updatedMessages = [...currentMessages];
        updatedMessages[existingIndex] = message;
        messagesSubject.next(updatedMessages);
      } else {
        // Otherwise, add the new message
        messagesSubject.next([...currentMessages, message]);
      }
    }
  }

  /**
   * Handle loading messages for a conversation
   */
  private handleLoadMessages(conversationId: string, messages: Message[]): void {
    if (!this.messageListeners.has(conversationId)) {
      this.messageListeners.set(conversationId, new BehaviorSubject<Message[]>([]));
    }
    this.messageListeners.get(conversationId).next(messages);
  }

  /**
   * Handle user started typing notification
   */
  private handleUserStartedTyping(conversationId: string, user: User): void {
    if (!this.typingUsersMap.has(conversationId)) {
      this.typingUsersMap.set(conversationId, new BehaviorSubject<User[]>([]));
    }
    
    const typingSubject = this.typingUsersMap.get(conversationId);
    const currentTypingUsers = typingSubject.getValue();
    
    // Only add if not already in the list
    if (!currentTypingUsers.some(u => u.id === user.id)) {
      typingSubject.next([...currentTypingUsers, user]);
    }
  }

  /**
   * Handle user stopped typing notification
   */
  private handleUserStoppedTyping(conversationId: string, user: User): void {
    if (this.typingUsersMap.has(conversationId)) {
      const typingSubject = this.typingUsersMap.get(conversationId);
      const currentTypingUsers = typingSubject.getValue();
      
      // Remove the user from typing list
      typingSubject.next(currentTypingUsers.filter(u => u.id !== user.id));
    }
  }

  /**
   * Handle alerts changed notification
   */
  private handleAlertsChanged(alerts: Alert[]): void {
    this.alertsSubject.next(alerts);
  }
}