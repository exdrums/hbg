import { Injectable } from "@angular/core";
import { AuthService } from "@app/core/services/auth.service";
import { ConfigService } from "@app/core/services/config.service";
import { SignalRAction, WsConnection } from "@app/core/services/websocket/ws-connection";
import { BehaviorSubject, Observable } from "rxjs";

/**
 * Enhanced SignalR Hub connection for Chat functionality
 *
 * This service manages the WebSocket connection to the chat microservice
 * following Angular and DevExtreme best practices for enterprise applications.
 *
 * Architecture:
 * - Implements facade pattern for clean API surface
 * - Uses dependency injection for ConfigService
 * - Provides strongly-typed observables for real-time events
 * - Implements proper resource cleanup
 *
 * Features:
 * - Real-time messaging with automatic reconnection
 * - Alert handling with typed event streams
 * - Multi-tab chat support (Contacts, Support, Agent)
 * - Typing indicators with auto-timeout
 * - User presence management
 * - Connection state monitoring
 *
 * Best Practices Applied:
 * - Single persistent connection shared across components
 * - Reactive state management with RxJS
 * - Typed interfaces for all data structures
 * - Comprehensive error handling and logging
 * - Memory leak prevention with cleanup methods
 *
 * @example
 * ```typescript
 * constructor(private chatWs: ChatWebSocketConnection) {}
 *
 * // Send message
 * await this.chatWs.sendMessage(conversationId, content);
 *
 * // Subscribe to typing indicators
 * this.chatWs.typing$.subscribe(event => {
 *   console.log(`${event.userId} is typing in ${event.conversationId}`);
 * });
 * ```
 */
@Injectable()
export class ChatWebSocketConnection extends WsConnection {

  // Connection state observables with typed interfaces
  private readonly _isTyping$ = new BehaviorSubject<{ conversationId: string; userId: string; isTyping: boolean } | null>(null);
  private readonly _userStatus$ = new BehaviorSubject<{ userId: string; status: string } | null>(null);
  private readonly _alerts$ = new BehaviorSubject<any>(null);

  // Typing indicator management
  private readonly typingTimeouts = new Map<string, NodeJS.Timeout>();
  private readonly currentlyTyping = new Set<string>();

  // Auto-timeout duration for typing indicators (milliseconds)
  private readonly TYPING_TIMEOUT_MS = 3000;

  constructor(
    protected readonly auth: AuthService,
    private readonly config: ConfigService
  ) {
    super(auth, auth.authStatus$);

    // Log connection configuration in development mode only
    if (!this.config.production) {
      console.debug('[ChatWS] Initializing connection to:', this.config.hbgcontacts);
    }
  }

  /**
   * Hub URL constructed from configuration
   * Uses the configured Contacts service endpoint
   */
  protected hubUrl: string = `${this.config.hbgcontacts}/hubs/chat`;

  /**
   * Connection permission check
   * Currently allows all authenticated users
   * Override this for custom connection logic
   */
  protected canConnect: () => boolean = () => true;

  //#region Public Observables

  /**
   * Observable for typing indicators
   */
  public get typing$(): Observable<{ conversationId: string; userId: string; isTyping: boolean } | null> {
    return this._isTyping$.asObservable();
  }

  /**
   * Observable for user status changes
   */
  public get userStatus$(): Observable<{ userId: string; status: string } | null> {
    return this._userStatus$.asObservable();
  }

  /**
   * Observable for alerts
   */
  public get alerts$(): Observable<any> {
    return this._alerts$.asObservable();
  }

  //#endregion

  //#region Chat Methods

  /**
   * Loads conversations for a specific type (Contacts, Support, Agent)
   *
   * Note: Method name matches backend hub method exactly (LoadConversation)
   * to ensure proper SignalR method resolution.
   *
   * @param loadOptions DevExtreme load options for pagination and filtering
   * @param conversationType Type of conversations to load (Contacts, Support, Agent)
   * @returns Promise with load result containing conversations
   */
  public async loadConversations(loadOptions: any, conversationType: string = 'Contacts'): Promise<any> {
    if (!this.config.production) {
      console.debug('[ChatWS] Loading conversations:', { conversationType, loadOptions });
    }

    // Backend method is 'LoadConversation' (singular) not 'LoadConversations'
    return await this.invoke('LoadConversation', loadOptions, conversationType);
  }

  /**
   * Creates a new conversation
   */
  public async createConversation(title: string, participantIds: string[], conversationType: string = 'Contacts'): Promise<any> {
    return await this.invoke('CreateConversation', title, participantIds, conversationType);
  }

  /**
   * Updates a conversation
   */
  public async updateConversation(conversationId: string, updates: any): Promise<void> {
    return await this.invoke('UpdateConversation', conversationId, updates);
  }

  /**
   * Archives a conversation
   */
  public async archiveConversation(conversationId: string): Promise<void> {
    return await this.invoke('ArchiveConversation', conversationId);
  }

  /**
   * Loads messages for a conversation
   */
  public async loadMessages(conversationId: string, loadOptions: any): Promise<any> {
    return await this.invoke('LoadMessages', conversationId, loadOptions);
  }

  /**
   * Sends a message
   */
  public async sendMessage(
    conversationId: string, 
    content: string, 
    type: string = 'Text',
    metadata?: string,
    replyToMessageId?: string
  ): Promise<any> {
    return await this.invoke('SendMessage', conversationId, content, type, metadata, replyToMessageId);
  }

  /**
   * Edits a message
   */
  public async editMessage(messageId: string, newContent: string): Promise<boolean> {
    return await this.invoke('EditMessage', messageId, newContent);
  }

  /**
   * Deletes a message
   */
  public async deleteMessage(messageId: string): Promise<boolean> {
    return await this.invoke('DeleteMessage', messageId);
  }

  /**
   * Marks a message as read
   */
  public async markMessageAsRead(messageId: string): Promise<void> {
    return await this.invoke('MarkMessageAsRead', messageId);
  }

  /**
   * Starts typing indicator
   */
  public async startTyping(conversationId: string): Promise<void> {
    if (!this.currentlyTyping.has(conversationId)) {
      this.currentlyTyping.add(conversationId);
      await this.invoke('StartTyping', conversationId);
      
      // Auto-stop typing after 3 seconds of inactivity
      this.resetTypingTimeout(conversationId);
    }
  }

  /**
   * Stops typing indicator
   */
  public async stopTyping(conversationId: string): Promise<void> {
    if (this.currentlyTyping.has(conversationId)) {
      this.currentlyTyping.delete(conversationId);
      this.clearTypingTimeout(conversationId);
      await this.invoke('StopTyping', conversationId);
    }
  }

  /**
   * Adds participants to a group conversation
   */
  public async addParticipants(conversationId: string, userIds: string[]): Promise<void> {
    return await this.invoke('AddParticipants', conversationId, userIds);
  }

  /**
   * Leaves a conversation
   */
  public async leaveConversation(conversationId: string): Promise<void> {
    return await this.invoke('LeaveConversation', conversationId);
  }

  /**
   * Removes a participant from a conversation
   */
  public async removeParticipant(conversationId: string, userId: string): Promise<void> {
    return await this.invoke('RemoveParticipant', conversationId, userId);
  }

  /**
   * Closes a support ticket (support agents only)
   */
  public async closeSupportTicket(conversationId: string, reason?: string): Promise<void> {
    return await this.invoke('CloseSupportTicket', conversationId, reason);
  }

  //#endregion

  //#region Event Handlers

  //#region SignalR Event Handlers

  /**
   * All event handlers use arrow functions to preserve 'this' context
   * when registered with SignalR. Handlers emit events to observables
   * which are consumed by data stores and components.
   */

  private readonly onMessageReceived = (message: any) => {
    this.logEvent('MessageReceived', message);
    // Handler will be picked up by EnhancedSignalRDataStore
  };

  private readonly onMessageEdited = (message: any) => {
    this.logEvent('MessageEdited', message);
  };

  private readonly onMessageDeleted = (messageId: string) => {
    this.logEvent('MessageDeleted', { messageId });
  };

  private readonly onMessageReadReceiptUpdated = (messageId: string, userId: string) => {
    this.logEvent('MessageReadReceiptUpdated', { messageId, userId });
  };

  private readonly onConversationCreated = (conversation: any) => {
    this.logEvent('ConversationCreated', conversation);
  };

  private readonly onConversationUpdated = (conversation: any) => {
    this.logEvent('ConversationUpdated', conversation);
  };

  private readonly onConversationArchived = (conversationId: string) => {
    this.logEvent('ConversationArchived', { conversationId });
  };

  private readonly onConversationLeft = (conversationId: string) => {
    this.logEvent('ConversationLeft', { conversationId });
  };

  private readonly onConversationAccessChanged = (conversationId: string, isReadOnly: boolean, reason: string) => {
    this.logEvent('ConversationAccessChanged', { conversationId, isReadOnly, reason });
  };

  private readonly onUserStatusChanged = (userId: string, status: string) => {
    this.logEvent('UserStatusChanged', { userId, status });
    this._userStatus$.next({ userId, status });
  };

  private readonly onUserStartedTyping = (conversationId: string, userId: string) => {
    this.logEvent('UserStartedTyping', { conversationId, userId });
    this._isTyping$.next({ conversationId, userId, isTyping: true });
  };

  private readonly onUserStoppedTyping = (conversationId: string, userId: string) => {
    this.logEvent('UserStoppedTyping', { conversationId, userId });
    this._isTyping$.next({ conversationId, userId, isTyping: false });
  };

  private readonly onReceiveAlert = (alert: any) => {
    this.logEvent('ReceiveAlert', alert);
    this._alerts$.next({ type: 'alert', ...alert });
  };

  private readonly onReceiveSystemAlert = (alert: any) => {
    this.logEvent('ReceiveSystemAlert', alert);
    this._alerts$.next({ type: 'system', ...alert });
  };

  private readonly onReceivePermissionAlert = (alert: any) => {
    this.logEvent('ReceivePermissionAlert', alert);
    this._alerts$.next({ type: 'permission', ...alert });
  };

  private readonly onConnectionStateChanged = (isConnected: boolean, message?: string) => {
    this.logEvent('ConnectionStateChanged', { isConnected, message });
    this._alerts$.next({
      type: 'connection',
      isConnected,
      content: message || (isConnected ? 'Connected' : 'Disconnected')
    });
  };

  private readonly onSupportTicketStatusChanged = (conversationId: string, status: string, assignedAgentId?: string) => {
    this.logEvent('SupportTicketStatusChanged', { conversationId, status, assignedAgentId });
  };

  private readonly onSupportTicketPriorityChanged = (conversationId: string, priority: string) => {
    this.logEvent('SupportTicketPriorityChanged', { conversationId, priority });
  };

  /**
   * Force refresh handler - reloads the application
   * Typically triggered for critical updates or configuration changes
   */
  private readonly onForceRefresh = (reason: string) => {
    this.logEvent('ForceRefresh', { reason }, 'warn');

    // Show warning before refresh
    if (confirm(`Application refresh required: ${reason}\n\nClick OK to refresh now.`)) {
      window.location.reload();
    }
  };

  private readonly onMaintenanceNotification = (message: string, startTime: Date, estimatedDuration: number) => {
    this.logEvent('MaintenanceNotification', { message, startTime, estimatedDuration }, 'warn');
    this._alerts$.next({
      type: 'maintenance',
      content: message,
      startTime,
      estimatedDuration
    });
  };

  private readonly onReloadDataSource = (dataSourceName: string, parameters?: any) => {
    this.logEvent('ReloadDataSource', { dataSourceName, parameters });
  };

  private readonly onPushDataSourceChanges = (dataSourceName: string, changes: any[]) => {
    this.logEvent('PushDataSourceChanges', { dataSourceName, changeCount: changes?.length });
  };

  private readonly onConversationDataSourceInsert = (conversation: any, conversationType: string) => {
    this.logEvent('ConversationDataSourceInsert', { conversationId: conversation?.conversationId, conversationType });
  };

  private readonly onConversationDataSourceUpdate = (conversation: any, conversationType: string) => {
    this.logEvent('ConversationDataSourceUpdate', { conversationId: conversation?.conversationId, conversationType });
  };

  private readonly onConversationDataSourceRemove = (conversationId: string, conversationType: string) => {
    this.logEvent('ConversationDataSourceRemove', { conversationId, conversationType });
  };

  //#endregion

  //#endregion

  //#region Private Helper Methods

  /**
   * Resets the typing timeout for a conversation
   * Auto-stops typing after configured timeout period
   */
  private resetTypingTimeout(conversationId: string): void {
    this.clearTypingTimeout(conversationId);

    const timeout = setTimeout(async () => {
      await this.stopTyping(conversationId);
    }, this.TYPING_TIMEOUT_MS);

    this.typingTimeouts.set(conversationId, timeout);
  }

  /**
   * Clears the typing timeout for a conversation
   */
  private clearTypingTimeout(conversationId: string): void {
    const timeout = this.typingTimeouts.get(conversationId);
    if (timeout) {
      clearTimeout(timeout);
      this.typingTimeouts.delete(conversationId);
    }
  }

  /**
   * Centralized logging for SignalR events
   * Only logs in development mode to reduce console noise in production
   *
   * @param eventName Name of the SignalR event
   * @param data Event data to log
   * @param level Log level (log, debug, warn, error)
   */
  private logEvent(eventName: string, data?: any, level: 'log' | 'debug' | 'warn' | 'error' = 'debug'): void {
    if (!this.config.production) {
      const message = `[ChatWS] ${eventName}`;

      switch (level) {
        case 'error':
          console.error(message, data);
          break;
        case 'warn':
          console.warn(message, data);
          break;
        case 'log':
          console.log(message, data);
          break;
        case 'debug':
        default:
          console.debug(message, data);
          break;
      }
    }
  }

    public override actions: SignalRAction[] = [
    // Message events
    { name: "MessageReceived", handler: this.onMessageReceived },
    { name: "MessageEdited", handler: this.onMessageEdited },
    { name: "MessageDeleted", handler: this.onMessageDeleted },
    { name: "MessageReadReceiptUpdated", handler: this.onMessageReadReceiptUpdated },
    
    // Conversation events
    { name: "ConversationCreated", handler: this.onConversationCreated },
    { name: "ConversationUpdated", handler: this.onConversationUpdated },
    { name: "ConversationArchived", handler: this.onConversationArchived },
    { name: "ConversationLeft", handler: this.onConversationLeft },
    { name: "ConversationAccessChanged", handler: this.onConversationAccessChanged },
    
    // User status events
    { name: "UserStatusChanged", handler: this.onUserStatusChanged },
    { name: "UserStartedTyping", handler: this.onUserStartedTyping },
    { name: "UserStoppedTyping", handler: this.onUserStoppedTyping },
    
    // Alert events
    { name: "ReceiveAlert", handler: this.onReceiveAlert },
    { name: "ReceiveSystemAlert", handler: this.onReceiveSystemAlert },
    { name: "ReceivePermissionAlert", handler: this.onReceivePermissionAlert },
    { name: "ConnectionStateChanged", handler: this.onConnectionStateChanged },
    
    // Support events
    { name: "SupportTicketStatusChanged", handler: this.onSupportTicketStatusChanged },
    { name: "SupportTicketPriorityChanged", handler: this.onSupportTicketPriorityChanged },
    
    // System events
    { name: "ForceRefresh", handler: this.onForceRefresh },
    { name: "MaintenanceNotification", handler: this.onMaintenanceNotification },
    
    // DevExtreme DataSource events
    { name: "ReloadDataSource", handler: this.onReloadDataSource },
    { name: "PushDataSourceChanges", handler: this.onPushDataSourceChanges },
    { name: "ConversationDataSourceInsert", handler: this.onConversationDataSourceInsert },
    { name: "ConversationDataSourceUpdate", handler: this.onConversationDataSourceUpdate },
    { name: "ConversationDataSourceRemove", handler: this.onConversationDataSourceRemove }
  ];

  /**
   * Override disconnect to clean up typing indicators
   */
  public disconnect() {
    // Clear all typing timeouts
    this.typingTimeouts.forEach(timeout => clearTimeout(timeout));
    this.typingTimeouts.clear();
    this.currentlyTyping.clear();
    
    super.disconnect();
  }

  //#endregion
}