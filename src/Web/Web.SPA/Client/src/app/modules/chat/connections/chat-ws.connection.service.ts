import { Injectable, inject } from "@angular/core";
import { AuthService } from "@app/core/services/auth.service";
import { ConfigService } from "@app/core/services/config.service";
import { SignalRAction, WsConnection } from "@app/core/services/websocket/ws-connection";
import { BehaviorSubject, Observable } from "rxjs";

/**
 * Enhanced SignalR Hub connection for Chat functionality
 * 
 * This service manages the WebSocket connection to the chat microservice
 * and provides enhanced features for:
 * - Real-time messaging
 * - Alert handling
 * - Multi-tab chat support (Contacts, Support, Agent)
 * - Typing indicators
 * - Presence management
 * 
 * The connection is shared across all chat-related data sources and components
 * to maintain a single persistent connection for optimal performance.
 */
@Injectable()
export class ChatWebSocketConnection extends WsConnection {
  private config: ConfigService = inject(ConfigService);
  
  // Connection state observables
  private _isTyping$ = new BehaviorSubject<{ conversationId: string; userId: string; isTyping: boolean } | null>(null);
  private _userStatus$ = new BehaviorSubject<{ userId: string; status: string } | null>(null);
  private _alerts$ = new BehaviorSubject<any>(null);
  
  // Typing indicator management
  private typingTimeouts = new Map<string, any>();
  private currentlyTyping = new Set<string>();

  constructor(protected readonly auth: AuthService) {
    super(auth, auth.authStatus$);
    console.log('this.config.hbgcontacts', this.config.hbgcontacts);
  }



  protected hubUrl: string = `${this.config.hbgcontacts}/hubs/chat`;
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
   */
  public async loadConversations(loadOptions: any, conversationType: string = 'Contacts'): Promise<any> {
    console.log('loadOptions', loadOptions);
    return await this.invoke('LoadConversations', loadOptions, conversationType);
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

  private readonly onMessageReceived = (message: any) => {
    console.log('Message received:', message);
    // Handler will be picked up by SignalR data stores
  };

  private readonly onMessageEdited = (message: any) => {
    console.log('Message edited:', message);
  };

  private readonly onMessageDeleted = (messageId: string) => {
    console.log('Message deleted:', messageId);
  };

  private readonly onMessageReadReceiptUpdated = (messageId: string, userId: string) => {
    console.log('Message read receipt updated:', messageId, userId);
  };

  private readonly onConversationCreated = (conversation: any) => {
    console.log('Conversation created:', conversation);
  };

  private readonly onConversationUpdated = (conversation: any) => {
    console.log('Conversation updated:', conversation);
  };

  private readonly onConversationArchived = (conversationId: string) => {
    console.log('Conversation archived:', conversationId);
  };

  private readonly onConversationLeft = (conversationId: string) => {
    console.log('Left conversation:', conversationId);
  };

  private readonly onConversationAccessChanged = (conversationId: string, isReadOnly: boolean, reason: string) => {
    console.log('Conversation access changed:', conversationId, isReadOnly, reason);
  };

  private readonly onUserStatusChanged = (userId: string, status: string) => {
    console.log('User status changed:', userId, status);
    this._userStatus$.next({ userId, status });
  };

  private readonly onUserStartedTyping = (conversationId: string, userId: string) => {
    console.log('User started typing:', conversationId, userId);
    this._isTyping$.next({ conversationId, userId, isTyping: true });
  };

  private readonly onUserStoppedTyping = (conversationId: string, userId: string) => {
    console.log('User stopped typing:', conversationId, userId);
    this._isTyping$.next({ conversationId, userId, isTyping: false });
  };

  private readonly onReceiveAlert = (alert: any) => {
    console.log('Alert received:', alert);
    this._alerts$.next({ type: 'alert', ...alert });
  };

  private readonly onReceiveSystemAlert = (alert: any) => {
    console.log('System alert received:', alert);
    this._alerts$.next({ type: 'system', ...alert });
  };

  private readonly onReceivePermissionAlert = (alert: any) => {
    console.log('Permission alert received:', alert);
    this._alerts$.next({ type: 'permission', ...alert });
  };

  private readonly onConnectionStateChanged = (isConnected: boolean, message?: string) => {
    console.log('Connection state changed:', isConnected, message);
    this._alerts$.next({ 
      type: 'connection', 
      isConnected, 
      content: message || (isConnected ? 'Connected' : 'Disconnected') 
    });
  };

  private readonly onSupportTicketStatusChanged = (conversationId: string, status: string, assignedAgentId?: string) => {
    console.log('Support ticket status changed:', conversationId, status, assignedAgentId);
  };

  private readonly onSupportTicketPriorityChanged = (conversationId: string, priority: string) => {
    console.log('Support ticket priority changed:', conversationId, priority);
  };

  private readonly onForceRefresh = (reason: string) => {
    console.log('Force refresh requested:', reason);
    window.location.reload();
  };

  private readonly onMaintenanceNotification = (message: string, startTime: Date, estimatedDuration: number) => {
    console.log('Maintenance notification:', message, startTime, estimatedDuration);
    this._alerts$.next({ 
      type: 'maintenance', 
      content: message, 
      startTime, 
      estimatedDuration 
    });
  };

  private readonly onReloadDataSource = (dataSourceName: string, parameters?: any) => {
    console.log('Reload data source:', dataSourceName, parameters);
  };

  private readonly onPushDataSourceChanges = (dataSourceName: string, changes: any[]) => {
    console.log('Push data source changes:', dataSourceName, changes);
  };

  private readonly onConversationDataSourceInsert = (conversation: any, conversationType: string) => {
    console.log('Conversation data source insert:', conversation, conversationType);
  };

  private readonly onConversationDataSourceUpdate = (conversation: any, conversationType: string) => {
    console.log('Conversation data source update:', conversation, conversationType);
  };

  private readonly onConversationDataSourceRemove = (conversationId: string, conversationType: string) => {
    console.log('Conversation data source remove:', conversationId, conversationType);
  };

  //#endregion

  //#region Private Helper Methods

  /**
   * Resets the typing timeout for a conversation
   */
  private resetTypingTimeout(conversationId: string): void {
    this.clearTypingTimeout(conversationId);
    
    const timeout = setTimeout(async () => {
      await this.stopTyping(conversationId);
    }, 3000); // Stop typing after 3 seconds of inactivity
    
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