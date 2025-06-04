import { Injectable } from '@angular/core';
import { CustomDataSource } from '@app/core/data/custom-data-source';
import { EnhancedSignalRDataStore } from '@app/core/data/enhanced-signalr-data-store';
import { BehaviorSubject, Observable, debounceTime, distinctUntilChanged } from 'rxjs';
import { ChatWebSocketConnection } from '../connections/chat-ws.connection.service';
import { AlertService } from '../services/alert.service';

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
 * This service provides a DevExtreme-compatible data source for chat messages
 * with real-time updates through SignalR. It supports the DxChat component
 * and provides comprehensive message management functionality.
 * 
 * Features:
 * - Real-time message updates via SignalR
 * - Message sending, editing, and deletion
 * - Read receipt management
 * - Typing indicators
 * - Message pagination and loading
 * - Integration with DxChat component
 * - Alert handling for message operations
 * 
 * The service maintains a single data source per conversation and provides
 * methods for all message-related operations required by the chat interface.
 */
@Injectable()
export class MessagesDataSourceService {
  
  private _dataSources = new Map<string, CustomDataSource<Message>>();
  private _currentConversationId$ = new BehaviorSubject<string | null>(null);
  private _isTyping$ = new BehaviorSubject<Map<string, string>>(new Map());
  private _typingSubject$ = new BehaviorSubject<string>('');

  // Typing indicator management
  private typingDebounceTime = 1000; // 1 second debounce

  constructor(
    private chatConnection: ChatWebSocketConnection,
    private alertService: AlertService
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
   * Set up typing indicator handlers
   */
  private setupTypingHandlers(): void {
    // Debounce typing input to avoid too frequent typing indicators
    this._typingSubject$.pipe(
      debounceTime(this.typingDebounceTime),
      distinctUntilChanged()
    ).subscribe(content => {
      const conversationId = this.currentConversationId;
      if (conversationId) {
        if (content.trim() === '') {
          // Stop typing when input is empty
          void this.stopTyping(conversationId);
        }
      }
    });

    // Start typing indicator on first keystroke
    this._typingSubject$.pipe(
      distinctUntilChanged()
    ).subscribe(content => {
      const conversationId = this.currentConversationId;
      if (conversationId && content.trim() !== '') {
        void this.startTyping(conversationId);
      }
    });
  }

  /**
   * Set up SignalR event handlers
   */
  private setupSignalRHandlers(): void {
    // Handle typing indicators from other users
    this.chatConnection.typing$.subscribe(typingEvent => {
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
   */
  private autoMarkMessagesAsRead(conversationId: string): void {
    const dataSource = this.getDataSource(conversationId);
    const messages = dataSource.items();
    const currentUserId = this.getCurrentUserId();
    
    // Mark recent unread messages as read
    const recentUnreadMessages = messages
      .filter(m => !m.readByUserIds.includes(currentUserId))
      .filter(m => m.senderUserId !== currentUserId) // Don't mark own messages
      .slice(-10); // Only mark the 10 most recent unread messages
    
    recentUnreadMessages.forEach(message => {
      void this.markMessageAsRead(message.messageId);
    });
  }

  /**
   * Get the current user ID
   */
  private getCurrentUserId(): string {
    // This should be obtained from your auth service
    // For now, return a placeholder
    return 'current-user-id'; // TODO: Replace with actual auth service
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
   */
  public destroy(): void {
    this._dataSources.forEach(dataSource => {
      dataSource.dispose();
    });
    
    this._dataSources.clear();
    this._currentConversationId$.complete();
    this._isTyping$.complete();
    this._typingSubject$.complete();
  }

  //#endregion
}