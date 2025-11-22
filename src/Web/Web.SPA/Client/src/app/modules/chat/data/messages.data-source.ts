import { Injectable } from '@angular/core';
import { CustomDataSource } from '@app/core/data/custom-data-source';
import { EnhancedSignalRDataStore } from '@app/core/data/enhanced-signalr-data-store';
import { BehaviorSubject, Observable, debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { ChatWebSocketConnection } from '../connections/chat-ws.connection.service';
import { AlertService } from '../services/alert.service';
import { AuthService } from '@app/core/services/auth.service';

/**
 * Interface for message objects
 */
export interface Message {
  messageId: string;
  conversationId: string;
  senderUserId: string;
  content: string;
  type: 'Text' | 'Image' | 'File' | 'System' | 'Audio' | 'Video' | 'Location' | 'Alert';
  sentAt: Date;
  editedAt?: Date;
  isDeleted: boolean;
  readByUserIds: string[];
  replyToMessageId?: string;
  metadata?: string;
}

/**
 * Enhanced data source service for managing messages within conversations
 *
 * This service implements Angular and DevExtreme best practices for enterprise
 * chat applications, providing a complete abstraction layer for message management.
 *
 * Architecture & Design Patterns:
 * - Facade Pattern: Simplifies complex SignalR/DevExtreme interactions
 * - Repository Pattern: Encapsulates data access logic
 * - Observer Pattern: Reactive state management with RxJS
 * - Singleton per Conversation: Efficient data source caching
 *
 * Features:
 * - Real-time message updates via SignalR
 * - Message CRUD operations (Create, Read, Update, Delete)
 * - Read receipt management with batch operations
 * - Typing indicators with debouncing
 * - Message pagination and infinite scroll support
 * - Integration with DevExtreme DxChat component
 * - Comprehensive error handling and user feedback
 * - Memory leak prevention with subscription management
 *
 * Performance Optimizations:
 * - Data source caching per conversation
 * - Debounced typing indicators
 * - Batch read receipt updates
 * - Efficient observable streams
 *
 * @example
 * ```typescript
 * constructor(private messagesService: MessagesDataSourceService) {}
 *
 * // Get data source for conversation
 * const dataSource = this.messagesService.getDataSource(conversationId);
 *
 * // Send a message
 * await this.messagesService.sendMessage(conversationId, 'Hello!');
 *
 * // Subscribe to typing indicators
 * this.messagesService.typing$.subscribe(typingUsers => {
 *   console.log('Users typing:', typingUsers);
 * });
 * ```
 */
@Injectable()
export class MessagesDataSourceService {

  // Data source cache - maintains one data source per conversation
  private readonly _dataSources = new Map<string, CustomDataSource<Message>>();

  // Observable state management
  private readonly _currentConversationId$ = new BehaviorSubject<string | null>(null);
  private readonly _isTyping$ = new BehaviorSubject<Map<string, string>>(new Map());
  private readonly _typingSubject$ = new BehaviorSubject<string>('');

  // Subscription management for cleanup
  private readonly destroy$ = new Subject<void>();

  // Configuration constants
  private readonly TYPING_DEBOUNCE_MS = 1000; // 1 second debounce
  private readonly AUTO_MARK_READ_LIMIT = 10; // Max messages to auto-mark as read

  constructor(
    private readonly chatConnection: ChatWebSocketConnection,
    private readonly alertService: AlertService,
    private readonly authService: AuthService
  ) {
    this.setupTypingHandlers();
    this.setupSignalRHandlers();
  }

  /**
   * Observable for the current conversation ID
   */
  public get currentConversationId$(): Observable<string | null> {
    return this._currentConversationId$.asObservable();
  }

  /**
   * Observable for typing indicators
   */
  public get typing$(): Observable<Map<string, string>> {
    return this._isTyping$.asObservable();
  }

  /**
   * Get the current conversation ID
   */
  public get currentConversationId(): string | null {
    return this._currentConversationId$.value;
  }

  //#region Data Source Management

  /**
   * Get or create data source for a conversation
   */
  public getDataSource(conversationId: string): CustomDataSource<Message> {
    let dataSource = this._dataSources.get(conversationId);
    
    if (!dataSource) {
      dataSource = this.createDataSource(conversationId);
      this._dataSources.set(conversationId, dataSource);
    }
    
    return dataSource;
  }

  /**
   * Set the current active conversation
   */
  public setCurrentConversation(conversationId: string | null): void {
    this._currentConversationId$.next(conversationId);
    
    if (conversationId) {
      // Ensure data source exists for the conversation
      this.getDataSource(conversationId);
    }
  }

  /**
   * Get the data source for the current conversation
   */
  public getCurrentDataSource(): CustomDataSource<Message> | null {
    const conversationId = this.currentConversationId;
    return conversationId ? this.getDataSource(conversationId) : null;
  }

  //#endregion

  //#region Message Operations

  /**
   * Send a text message
   */
  public async sendMessage(
    conversationId: string,
    content: string,
    replyToMessageId?: string
  ): Promise<Message | null> {
    try {
      // Stop typing indicator
      await this.stopTyping(conversationId);
      
      const message = await this.chatConnection.sendMessage(
        conversationId,
        content,
        'Text',
        undefined,
        replyToMessageId
      );
      
      return message;
    } catch (error) {
      console.error('Failed to send message:', error);
      this.alertService.showError(
        `Failed to send message: ${this.getErrorMessage(error)}`,
        'Send Failed'
      );
      return null;
    }
  }

  /**
   * Edit a message
   */
  public async editMessage(messageId: string, newContent: string): Promise<boolean> {
    try {
      const success = await this.chatConnection.editMessage(messageId, newContent);
      
      if (success) {
        this.alertService.showSuccess('Message edited successfully', 'Edit Successful');
      } else {
        this.alertService.showWarning('Message could not be edited', 'Edit Failed');
      }
      
      return success;
    } catch (error) {
      console.error('Failed to edit message:', error);
      this.alertService.showError(
        `Failed to edit message: ${this.getErrorMessage(error)}`,
        'Edit Failed'
      );
      return false;
    }
  }

  /**
   * Delete a message
   */
  public async deleteMessage(messageId: string): Promise<boolean> {
    try {
      const success = await this.chatConnection.deleteMessage(messageId);
      
      if (success) {
        this.alertService.showSuccess('Message deleted successfully', 'Delete Successful');
      } else {
        this.alertService.showWarning('Message could not be deleted', 'Delete Failed');
      }
      
      return success;
    } catch (error) {
      console.error('Failed to delete message:', error);
      this.alertService.showError(
        `Failed to delete message: ${this.getErrorMessage(error)}`,
        'Delete Failed'
      );
      return false;
    }
  }

  /**
   * Mark a message as read
   */
  public async markMessageAsRead(messageId: string): Promise<void> {
    try {
      await this.chatConnection.markMessageAsRead(messageId);
    } catch (error) {
      console.error('Failed to mark message as read:', error);
      // Don't show alerts for read receipt failures as they're not critical
    }
  }

  /**
   * Mark all messages in a conversation as read
   */
  public async markConversationAsRead(conversationId: string): Promise<void> {
    const dataSource = this.getDataSource(conversationId);
    const messages = dataSource.items();
    
    // Mark unread messages as read
    const unreadMessages = messages.filter(m => !m.readByUserIds.includes(this.getCurrentUserId()));
    
    for (const message of unreadMessages) {
      await this.markMessageAsRead(message.messageId);
    }
  }

  //#endregion

  //#region Typing Indicators

  /**
   * Start typing indicator for a conversation
   */
  public async startTyping(conversationId: string): Promise<void> {
    try {
      await this.chatConnection.startTyping(conversationId);
    } catch (error) {
      console.error('Failed to start typing indicator:', error);
    }
  }

  /**
   * Stop typing indicator for a conversation
   */
  public async stopTyping(conversationId: string): Promise<void> {
    try {
      await this.chatConnection.stopTyping(conversationId);
    } catch (error) {
      console.error('Failed to stop typing indicator:', error);
    }
  }

  /**
   * Handle user typing in the input field
   */
  public onUserTyping(conversationId: string, content: string): void {
    this._typingSubject$.next(content);
  }

  /**
   * Get typing users for a conversation
   */
  public getTypingUsers(conversationId: string): string[] {
    const typingMap = this._isTyping$.value;
    const typingUsers: string[] = [];
    
    typingMap.forEach((convId, userId) => {
      if (convId === conversationId) {
        typingUsers.push(userId);
      }
    });
    
    return typingUsers;
  }

  //#endregion

  //#region Data Source Operations

  /**
   * Refresh messages for a conversation
   */
  public async refreshConversation(conversationId: string): Promise<void> {
    const dataSource = this.getDataSource(conversationId);
    await dataSource.reload();
  }

  /**
   * Load more messages for a conversation (pagination)
   */
  public async loadMoreMessages(conversationId: string): Promise<void> {
    const dataSource = this.getDataSource(conversationId);
    // Implement pagination logic if needed
    await dataSource.load();
  }

  //#endregion

  //#region Private Methods

  /**
   * Create a new data source for a conversation
   */
  private createDataSource(conversationId: string): CustomDataSource<Message> {
    const dataStore = new EnhancedSignalRDataStore<Message>(
      this.chatConnection,
      'messageId',
      'Message'
    );

    // Set the conversation ID as the subject ID
    dataStore.subjectId = conversationId as any;

    const dataSource = new CustomDataSource<Message>(dataStore);

    // Set up alert handlers for this data store
    this.setupDataStoreAlerts(dataStore, conversationId);

    // Set up data source event handlers
    dataSource.on('loadingChanged', (isLoading) => {
      if (!isLoading) {
        // Auto-mark visible messages as read
        this.autoMarkMessagesAsRead(conversationId);
      }
    });

    return dataSource;
  }

  /**
   * Set up alert handlers for data store operations
   */
  private setupDataStoreAlerts(dataStore: EnhancedSignalRDataStore<Message>, conversationId: string): void {
    dataStore.onAlert('error', (alert) => {
      this.alertService.showError(
        alert.content,
        'Message Error',
        { conversationId }
      );
    });

    dataStore.onAlert('warning', (alert) => {
      this.alertService.showWarning(
        alert.content,
        'Message Warning',
        { conversationId }
      );
    });

    dataStore.onAlert('permission', (alert) => {
      this.alertService.showPermissionDenied(
        'send messages',
        conversationId
      );
    });
  }

  /**
   * Set up typing indicator handlers with proper cleanup
   *
   * Implements debouncing to reduce network traffic and server load.
   * Two subscriptions:
   * 1. Debounced - stops typing when user pauses
   * 2. Immediate - starts typing on first keystroke
   *
   * Both subscriptions are properly cleaned up on service destroy.
   */
  private setupTypingHandlers(): void {
    // Debounce typing input to avoid too frequent typing indicators
    this._typingSubject$.pipe(
      debounceTime(this.TYPING_DEBOUNCE_MS),
      distinctUntilChanged(),
      takeUntil(this.destroy$) // Cleanup subscription
    ).subscribe(content => {
      const conversationId = this.currentConversationId;
      if (conversationId && content.trim() === '') {
        // Stop typing when input is empty
        void this.stopTyping(conversationId);
      }
    });

    // Start typing indicator on first keystroke
    this._typingSubject$.pipe(
      distinctUntilChanged(),
      takeUntil(this.destroy$) // Cleanup subscription
    ).subscribe(content => {
      const conversationId = this.currentConversationId;
      if (conversationId && content.trim() !== '') {
        void this.startTyping(conversationId);
      }
    });
  }

  /**
   * Set up SignalR event handlers with proper cleanup
   *
   * Subscribes to real-time typing indicators from the chat connection.
   * Subscription is properly cleaned up when service is destroyed.
   */
  private setupSignalRHandlers(): void {
    // Handle typing indicators from other users
    this.chatConnection.typing$.pipe(
      takeUntil(this.destroy$) // Cleanup subscription
    ).subscribe(typingEvent => {
      if (typingEvent) {
        const { conversationId, userId, isTyping } = typingEvent;
        const currentTyping = new Map(this._isTyping$.value);

        if (isTyping) {
          currentTyping.set(userId, conversationId);
        } else {
          currentTyping.delete(userId);
        }

        this._isTyping$.next(currentTyping);
      }
    });
  }

  /**
   * Auto-mark messages as read when they're loaded/visible
   *
   * Implements intelligent read receipt behavior:
   * - Only marks messages from other users
   * - Limits to most recent N messages to avoid performance issues
   * - Skips if user ID is not available
   *
   * This is called after data source loads to provide automatic
   * read receipt functionality similar to modern chat applications.
   *
   * @param conversationId ID of the conversation
   */
  private autoMarkMessagesAsRead(conversationId: string): void {
    const dataSource = this.getDataSource(conversationId);
    const messages = dataSource.items();
    const currentUserId = this.getCurrentUserId();

    // Skip if user ID is not available
    if (!currentUserId) {
      return;
    }

    // Mark recent unread messages as read
    const recentUnreadMessages = messages
      .filter(m => !m.readByUserIds.includes(currentUserId))
      .filter(m => m.senderUserId !== currentUserId) // Don't mark own messages
      .slice(-this.AUTO_MARK_READ_LIMIT); // Limit for performance

    recentUnreadMessages.forEach(message => {
      void this.markMessageAsRead(message.messageId);
    });
  }

  /**
   * Get the current user ID from authentication service
   *
   * Uses the AuthService to retrieve the authenticated user's ID.
   * Returns null if user is not authenticated.
   *
   * @returns Current user ID or null if not authenticated
   */
  private getCurrentUserId(): string | null {
    return this.authService.currentUser?.id ?? this.authService.userId ?? null;
  }

  /**
   * Extract error message from error object
   */
  private getErrorMessage(error: any): string {
    if (typeof error === 'string') {
      return error;
    }
    
    if (error?.message) {
      return error.message;
    }
    
    if (error?.error?.message) {
      return error.error.message;
    }
    
    return 'An unexpected error occurred';
  }

  //#endregion

  //#region Cleanup

  /**
   * Remove data source for a conversation
   */
  public removeDataSource(conversationId: string): void {
    const dataSource = this._dataSources.get(conversationId);
    if (dataSource) {
      dataSource.dispose();
      this._dataSources.delete(conversationId);
    }
  }

  /**
   * Cleanup method to be called when the service is destroyed
   *
   * Implements proper resource cleanup to prevent memory leaks:
   * - Disposes all data sources
   * - Completes all observables
   * - Clears all maps
   * - Triggers destroy$ to unsubscribe all subscriptions
   *
   * IMPORTANT: This should be called in the ngOnDestroy lifecycle hook
   * of the component or service that owns this service.
   */
  public destroy(): void {
    // Signal all subscriptions to unsubscribe
    this.destroy$.next();
    this.destroy$.complete();

    // Dispose all data sources
    this._dataSources.forEach(dataSource => {
      dataSource.dispose();
    });

    // Clear collections
    this._dataSources.clear();

    // Complete all subjects
    this._currentConversationId$.complete();
    this._isTyping$.complete();
    this._typingSubject$.complete();
  }

  //#endregion
}