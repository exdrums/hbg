import { Injectable } from '@angular/core';
import { CustomDataSource } from '@app/core/data/custom-data-source';
import { EnhancedSignalRDataStore } from '@app/core/data/enhanced-signalr-data-store';
import { BehaviorSubject, Observable } from 'rxjs';
import { ChatWebSocketConnection } from '../connections/chat-ws.connection.service';
import { AlertService } from '../services/alert.service';

/**
 * Interface for conversation objects
 */
export interface Conversation {
  conversationId: string;
  title?: string;
  type: 'Direct' | 'Group' | 'System';
  participantIds: string[];
  createdByUserId: string;
  createdAt: Date;
  lastMessageAt?: Date;
  isActive: boolean;
  lastMessagePreview?: string;
  unreadCount: number;
}

/**
 * Conversation types for different tabs
 */
export enum ConversationType {
  Contacts = 'Contacts',
  Support = 'Support',
  Agent = 'Agent'
}

/**
 * Enhanced data source service for managing conversations
 * 
 * This service provides a DevExtreme-compatible data source for conversation lists
 * with real-time updates through SignalR. It supports different conversation types
 * for the tabbed interface (Contacts, Support, Agent).
 * 
 * Features:
 * - Real-time conversation updates via SignalR
 * - Support for different conversation types/tabs
 * - Conversation creation and management
 * - Alert integration for user feedback
 * - Automatic cache management
 * - DevExtreme DataGrid integration
 * 
 * The service maintains separate data stores for each conversation type
 * to support the multi-tab chat interface efficiently.
 */
@Injectable()
export class ConversationsDataSourceService {
  
  private _dataSources = new Map<ConversationType, CustomDataSource<Conversation>>();
  private _selectedConversation$ = new BehaviorSubject<Conversation | null>(null);
  private _conversationCounts$ = new BehaviorSubject<Record<ConversationType, number>>({
    [ConversationType.Contacts]: 0,
    [ConversationType.Support]: 0,
    [ConversationType.Agent]: 0
  });

  constructor(
    private chatConnection: ChatWebSocketConnection,
    private alertService: AlertService
  ) {
    this.initializeDataSources();
    this.setupAlertHandlers();
  }

  /**
   * Observable for the currently selected conversation
   */
  public get selectedConversation$(): Observable<Conversation | null> {
    return this._selectedConversation$.asObservable();
  }

  /**
   * Observable for conversation counts by type
   */
  public get conversationCounts$(): Observable<Record<ConversationType, number>> {
    return this._conversationCounts$.asObservable();
  }

  /**
   * Get the currently selected conversation
   */
  public get selectedConversation(): Conversation | null {
    return this._selectedConversation$.value;
  }

  //#region Data Source Access

  /**
   * Get data source for a specific conversation type
   */
  public getDataSource(type: ConversationType): CustomDataSource<Conversation> {
    const dataSource = this._dataSources.get(type);
    if (!dataSource) {
      throw new Error(`Data source for conversation type ${type} not found`);
    }
    return dataSource;
  }

  /**
   * Get data source for Contacts conversations
   */
  public get contactsDataSource(): CustomDataSource<Conversation> {
    return this.getDataSource(ConversationType.Contacts);
  }

  /**
   * Get data source for Support conversations
   */
  public get supportDataSource(): CustomDataSource<Conversation> {
    return this.getDataSource(ConversationType.Support);
  }

  /**
   * Get data source for Agent conversations
   */
  public get agentDataSource(): CustomDataSource<Conversation> {
    return this.getDataSource(ConversationType.Agent);
  }

  //#endregion

  //#region Conversation Management

  /**
   * Creates a new conversation
   */
  public async createConversation(
    title: string,
    participantIds: string[],
    type: ConversationType = ConversationType.Contacts
  ): Promise<Conversation | null> {
    try {
      const conversation = await this.chatConnection.createConversation(title, participantIds, type);
      
      this.alertService.showSuccess(
        `${type} conversation created successfully`,
        'Conversation Created'
      );
      
      return conversation;
    } catch (error) {
      console.error('Failed to create conversation:', error);
      this.alertService.showError(
        `Failed to create ${type.toLowerCase()} conversation: ${this.getErrorMessage(error)}`,
        'Creation Failed'
      );
      return null;
    }
  }

  /**
   * Creates a new direct conversation (Contacts)
   */
  public async createDirectConversation(otherUserId: string): Promise<Conversation | null> {
    return this.createConversation('', [otherUserId], ConversationType.Contacts);
  }

  /**
   * Creates a new group conversation
   */
  public async createGroupConversation(
    title: string,
    participantIds: string[],
    type: ConversationType = ConversationType.Contacts
  ): Promise<Conversation | null> {
    if (!title.trim()) {
      this.alertService.showWarning('Group conversations must have a title', 'Title Required');
      return null;
    }
    
    if (participantIds.length < 2) {
      this.alertService.showWarning('Group conversations must have at least 2 participants', 'Insufficient Participants');
      return null;
    }
    
    return this.createConversation(title, participantIds, type);
  }

  /**
   * Creates a new support ticket
   */
  public async createSupportTicket(subject: string): Promise<Conversation | null> {
    return this.createConversation(subject, [], ConversationType.Support);
  }

  /**
   * Creates a new AI agent conversation
   */
  public async createAgentConversation(topic: string = 'New Chat'): Promise<Conversation | null> {
    return this.createConversation(topic, [], ConversationType.Agent);
  }

  /**
   * Updates a conversation
   */
  public async updateConversation(conversationId: string, updates: any): Promise<boolean> {
    try {
      await this.chatConnection.updateConversation(conversationId, updates);
      
      this.alertService.showSuccess('Conversation updated successfully', 'Update Successful');
      return true;
    } catch (error) {
      console.error('Failed to update conversation:', error);
      this.alertService.showError(
        `Failed to update conversation: ${this.getErrorMessage(error)}`,
        'Update Failed'
      );
      return false;
    }
  }

  /**
   * Archives a conversation
   */
  public async archiveConversation(conversationId: string): Promise<boolean> {
    try {
      await this.chatConnection.archiveConversation(conversationId);
      
      // Clear selection if the archived conversation was selected
      if (this.selectedConversation?.conversationId === conversationId) {
        this.setSelectedConversation(null);
      }
      
      this.alertService.showSuccess('Conversation archived successfully', 'Archive Successful');
      return true;
    } catch (error) {
      console.error('Failed to archive conversation:', error);
      this.alertService.showError(
        `Failed to archive conversation: ${this.getErrorMessage(error)}`,
        'Archive Failed'
      );
      return false;
    }
  }

  /**
   * Leaves a conversation
   */
  public async leaveConversation(conversationId: string): Promise<boolean> {
    try {
      await this.chatConnection.leaveConversation(conversationId);
      
      // Clear selection if the left conversation was selected
      if (this.selectedConversation?.conversationId === conversationId) {
        this.setSelectedConversation(null);
      }
      
      return true;
    } catch (error) {
      console.error('Failed to leave conversation:', error);
      this.alertService.showError(
        `Failed to leave conversation: ${this.getErrorMessage(error)}`,
        'Leave Failed'
      );
      return false;
    }
  }

  //#endregion

  //#region Selection Management

  /**
   * Sets the currently selected conversation
   */
  public setSelectedConversation(conversation: Conversation | null): void {
    this._selectedConversation$.next(conversation);
  }

  /**
   * Selects a conversation by ID
   */
  public selectConversationById(conversationId: string, type: ConversationType): void {
    const dataSource = this.getDataSource(type);
    const items = dataSource.items();
    const conversation = items.find(c => c.conversationId === conversationId);
    
    if (conversation) {
      this.setSelectedConversation(conversation);
    } else {
      console.warn(`Conversation with ID ${conversationId} not found in ${type} data source`);
    }
  }

  //#endregion

  //#region Data Source Refresh

  /**
   * Refreshes all conversation data sources
   */
  public async refreshAll(): Promise<void> {
    const refreshPromises = Array.from(this._dataSources.values()).map(ds => ds.reload());
    await Promise.all(refreshPromises);
    this.updateConversationCounts();
  }

  /**
   * Refreshes a specific conversation type data source
   */
  public async refreshConversationType(type: ConversationType): Promise<void> {
    const dataSource = this.getDataSource(type);
    await dataSource.reload();
    this.updateConversationCounts();
  }

  //#endregion

  //#region Private Methods

  /**
   * Initialize data sources for all conversation types
   */
  private initializeDataSources(): void {
    Object.values(ConversationType).forEach(type => {
      const dataStore = new EnhancedSignalRDataStore<Conversation>(
        this.chatConnection,
        'conversationId',
        'Conversation',
        type
      );

      const dataSource = new CustomDataSource<Conversation>(dataStore);
      
      // Set up alert handlers for this data store
      this.setupDataStoreAlerts(dataStore, type);
      
      // Set up data source event handlers
      dataSource.on('loadingChanged', (isLoading) => {
        if (!isLoading) {
          this.updateConversationCounts();
        }
      });

      this._dataSources.set(type, dataSource);
    });
  }

  /**
   * Set up alert handlers for data store operations
   */
  private setupDataStoreAlerts(dataStore: EnhancedSignalRDataStore<Conversation>, type: ConversationType): void {
    // Handle different alert types from the data store
    dataStore.onAlert('error', (alert) => {
      this.alertService.showError(
        alert.content,
        `${type} Error`
      );
    });

    dataStore.onAlert('warning', (alert) => {
      this.alertService.showWarning(
        alert.content,
        `${type} Warning`
      );
    });

    dataStore.onAlert('info', (alert) => {
      this.alertService.showInfo(
        alert.content,
        `${type} Info`
      );
    });

    dataStore.onAlert('connection', (alert) => {
      this.alertService.showConnectionAlert(
        alert.isConnected,
        alert.message
      );
    });
  }

  /**
   * Set up global alert handlers
   */
  private setupAlertHandlers(): void {
    // Handle connection state changes
    this.chatConnection.alerts$.subscribe(alert => {
      if (alert?.type === 'connection') {
        // Connection alerts are handled by individual data stores
        return;
      }
    });
  }

  /**
   * Update conversation counts for all types
   */
  private updateConversationCounts(): void {
    const counts: Record<ConversationType, number> = {
      [ConversationType.Contacts]: 0,
      [ConversationType.Support]: 0,
      [ConversationType.Agent]: 0
    };

    this._dataSources.forEach((dataSource, type) => {
      counts[type] = dataSource.items().length;
    });

    this._conversationCounts$.next(counts);
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
   * Cleanup method to be called when the service is destroyed
   */
  public destroy(): void {
    this._dataSources.forEach(dataSource => {
      dataSource.dispose();
    });
    
    this._dataSources.clear();
    this._selectedConversation$.complete();
    this._conversationCounts$.complete();
  }

  //#endregion
}