import { Injectable } from '@angular/core';
import { CustomDataSource } from '@app/core/data/custom-data-source';
import { EnhancedSignalRDataStore } from '@app/core/data/enhanced-signalr-data-store';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { ChatWebSocketConnection } from '../connections/chat-ws.connection.service';
import { AgentWebSocketConnection } from '../connections/agent-ws.connection.service';
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
  // Agent-specific properties
  agentType?: string;
  modelName?: string;
  tokenUsage?: {
    used: number;
    remaining: number;
    limit: number;
  };
  context?: any;
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
 * Enhanced conversation data source service with multi-connection support
 * 
 * This enhanced version supports both the main chat connection and a separate
 * AI agent connection, providing:
 * - Unified interface for all conversation types
 * - Separate connections for different microservices
 * - AI-specific features (token tracking, model selection, etc.)
 * - Real-time updates for both regular and AI conversations
 * - Context management for AI conversations
 * 
 * The service automatically routes operations to the appropriate connection
 * based on conversation type, ensuring optimal performance and separation
 * of concerns between regular chat and AI functionality.
 */
@Injectable()
export class EnhancedConversationsDataSourceService {
  
  private _dataSources = new Map<ConversationType, CustomDataSource<Conversation>>();
  private _selectedConversation$ = new BehaviorSubject<Conversation | null>(null);
  private _conversationCounts$ = new BehaviorSubject<Record<ConversationType, number>>({
    [ConversationType.Contacts]: 0,
    [ConversationType.Support]: 0,
    [ConversationType.Agent]: 0
  });

  // AI-specific state
  private _agentThinking$ = new BehaviorSubject<Set<string>>(new Set());
  private _tokenLimits$ = new BehaviorSubject<Map<string, any>>(new Map());

  constructor(
    private chatConnection: ChatWebSocketConnection,
    private agentConnection: AgentWebSocketConnection,
    private alertService: AlertService
  ) {
    this.initializeDataSources();
    this.setupAlertHandlers();
    this.setupAgentEventHandlers();
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
   * Observable for AI thinking status
   */
  public get agentThinking$(): Observable<Set<string>> {
    return this._agentThinking$.asObservable();
  }

  /**
   * Observable for token usage across conversations
   */
  public get tokenLimits$(): Observable<Map<string, any>> {
    return this._tokenLimits$.asObservable();
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

  //#region Connection Routing

  /**
   * Get the appropriate connection for a conversation type
   */
  private getConnectionForType(type: ConversationType): ChatWebSocketConnection | AgentWebSocketConnection {
    return type === ConversationType.Agent ? this.agentConnection : this.chatConnection;
  }

  //#endregion

  //#region Conversation Management

  /**
   * Creates a new conversation with automatic connection routing
   */
  public async createConversation(
    title: string,
    participantIds: string[],
    type: ConversationType = ConversationType.Contacts,
    options?: { agentType?: string; modelName?: string; context?: any }
  ): Promise<Conversation | null> {
    try {
      let conversation: Conversation | null = null;
      
      if (type === ConversationType.Agent) {
        // Use agent connection for AI conversations
        conversation = await this.agentConnection.createAgentConversation(
          title, 
          options?.agentType || 'general'
        );
        
        // Set additional AI properties if specified
        if (options?.modelName || options?.context) {
          await this.updateAgentConversationSettings(
            conversation.conversationId, 
            options.modelName, 
            options.context
          );
        }
      } else {
        // Use chat connection for regular conversations
        conversation = await this.chatConnection.createConversation(title, participantIds, type);
      }
      
      if (conversation) {
        this.alertService.showSuccess(
          `${type} conversation created successfully`,
          'Conversation Created'
        );
      }
      
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
   * Creates a new support ticket
   */
  public async createSupportTicket(subject: string): Promise<Conversation | null> {
    return this.createConversation(subject, [], ConversationType.Support);
  }

  /**
   * Creates a new AI agent conversation with specific settings
   */
  public async createAgentConversation(
    topic: string = 'New Chat',
    agentType: string = 'general',
    modelName?: string
  ): Promise<Conversation | null> {
    return this.createConversation(topic, [], ConversationType.Agent, {
      agentType,
      modelName
    });
  }

  /**
   * Updates AI agent conversation settings
   */
  public async updateAgentConversationSettings(
    conversationId: string,
    modelName?: string,
    context?: any
  ): Promise<boolean> {
    try {
      if (modelName) {
        await this.agentConnection.switchAgentModel(conversationId, modelName);
      }
      
      if (context) {
        await this.agentConnection.updateConversationContext(conversationId, context);
      }
      
      this.alertService.showSuccess('AI settings updated successfully', 'Settings Updated');
      return true;
    } catch (error) {
      console.error('Failed to update AI settings:', error);
      this.alertService.showError(
        `Failed to update AI settings: ${this.getErrorMessage(error)}`,
        'Update Failed'
      );
      return false;
    }
  }

  /**
   * Resets AI conversation context
   */
  public async resetAgentContext(conversationId: string): Promise<boolean> {
    try {
      await this.agentConnection.resetConversationContext(conversationId);
      this.alertService.showSuccess('AI context reset successfully', 'Context Reset');
      return true;
    } catch (error) {
      console.error('Failed to reset AI context:', error);
      this.alertService.showError(
        `Failed to reset AI context: ${this.getErrorMessage(error)}`,
        'Reset Failed'
      );
      return false;
    }
  }

  /**
   * Updates a conversation using the appropriate connection
   */
  public async updateConversation(
    conversationId: string, 
    updates: any, 
    type: ConversationType
  ): Promise<boolean> {
    try {
      const connection = this.getConnectionForType(type);
      
      if (type === ConversationType.Agent && connection instanceof AgentWebSocketConnection) {
        // Handle AI-specific updates
        if (updates.modelName) {
          await connection.switchAgentModel(conversationId, updates.modelName);
        }
        if (updates.context) {
          await connection.updateConversationContext(conversationId, updates.context);
        }
      } else {
        // Handle regular conversation updates
        await (connection as ChatWebSocketConnection).updateConversation(conversationId, updates);
      }
      
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

  //#region AI-Specific Methods

  /**
   * Check if an AI agent is currently thinking/processing
   */
  public isAgentThinking(conversationId: string): boolean {
    return this._agentThinking$.value.has(conversationId);
  }

  /**
   * Get token usage for an AI conversation
   */
  public getTokenUsage(conversationId: string): any | null {
    return this._tokenLimits$.value.get(conversationId) || null;
  }

  /**
   * Check if token limit is approaching
   */
  public isTokenLimitApproaching(conversationId: string, threshold: number = 100): boolean {
    const usage = this.getTokenUsage(conversationId);
    return usage ? usage.tokensRemaining <= threshold : false;
  }

  /**
   * Submit feedback for AI response
   */
  public async submitAIFeedback(
    messageId: string, 
    rating: number, 
    feedback?: string
  ): Promise<boolean> {
    try {
      await this.agentConnection.submitAIFeedback(messageId, rating, feedback);
      this.alertService.showSuccess('Thank you for your feedback!', 'Feedback Submitted');
      return true;
    } catch (error) {
      console.error('Failed to submit AI feedback:', error);
      this.alertService.showError('Failed to submit feedback', 'Submission Failed');
      return false;
    }
  }

  /**
   * Get available AI agents/models
   */
  public async getAvailableAgents(): Promise<any[]> {
    try {
      return await this.agentConnection.getAvailableAgents();
    } catch (error) {
      console.error('Failed to get available agents:', error);
      return [];
    }
  }

  //#endregion

  //#region Event Handlers Setup

  /**
   * Initialize data sources for all conversation types
   */
  private initializeDataSources(): void {
    // Contacts and Support use chat connection
    [ConversationType.Contacts, ConversationType.Support].forEach(type => {
      const dataStore = new EnhancedSignalRDataStore<Conversation>(
        this.chatConnection,
        'conversationId',
        'Conversation',
        type
      );

      const dataSource = new CustomDataSource<Conversation>(dataStore);
      this.setupDataStoreAlerts(dataStore, type);
      this.setupDataSourceEvents(dataSource);
      this._dataSources.set(type, dataSource);
    });

    // Agent uses agent connection
    const agentDataStore = new EnhancedSignalRDataStore<Conversation>(
      this.agentConnection,
      'conversationId',
      'Conversation',
      ConversationType.Agent
    );

    const agentDataSource = new CustomDataSource<Conversation>(agentDataStore);
    this.setupDataStoreAlerts(agentDataStore, ConversationType.Agent);
    this.setupDataSourceEvents(agentDataSource);
    this._dataSources.set(ConversationType.Agent, agentDataSource);
  }

  /**
   * Set up alert handlers for data stores
   */
  private setupDataStoreAlerts(dataStore: EnhancedSignalRDataStore<Conversation>, type: ConversationType): void {
    dataStore.onAlert('error', (alert) => {
      this.alertService.showError(alert.content, `${type} Error`);
    });

    dataStore.onAlert('warning', (alert) => {
      this.alertService.showWarning(alert.content, `${type} Warning`);
    });

    dataStore.onAlert('info', (alert) => {
      this.alertService.showInfo(alert.content, `${type} Info`);
    });
  }

  /**
   * Set up data source event handlers
   */
  private setupDataSourceEvents(dataSource: CustomDataSource<Conversation>): void {
    dataSource.on('loadingChanged', (isLoading) => {
      if (!isLoading) {
        this.updateConversationCounts();
      }
    });
  }

  /**
   * Set up global alert handlers
   */
  private setupAlertHandlers(): void {
    // Chat connection alerts are handled by individual data stores
    this.chatConnection.alerts$.subscribe(alert => {
      if (alert?.type === 'connection') {
        // Connection alerts are handled by AlertService directly
        return;
      }
    });
  }

  /**
   * Set up AI agent-specific event handlers
   */
  private setupAgentEventHandlers(): void {
    // Handle AI thinking indicators
    this.agentConnection.aiThinking$.subscribe(thinking => {
      if (thinking) {
        const currentThinking = new Set(this._agentThinking$.value);
        if (thinking.isThinking) {
          currentThinking.add(thinking.conversationId);
        } else {
          currentThinking.delete(thinking.conversationId);
        }
        this._agentThinking$.next(currentThinking);
      }
    });

    // Handle token usage updates
    this.agentConnection.tokenUsage$.subscribe(usage => {
      if (usage) {
        const currentLimits = new Map(this._tokenLimits$.value);
        currentLimits.set(usage.conversationId, {
          tokensUsed: usage.tokensUsed,
          tokensRemaining: usage.tokensRemaining
        });
        this._tokenLimits$.next(currentLimits);
      }
    });

    // Handle agent errors
    this.agentConnection.agentErrors$.subscribe(error => {
      if (error) {
        switch (error.type) {
          case 'token_limit_reached':
            this.alertService.showError(error.message, 'Token Limit Reached');
            break;
          case 'token_limit_warning':
            this.alertService.showWarning(error.message, 'Token Limit Warning');
            break;
          case 'agent_unavailable':
            this.alertService.showError(error.message, 'AI Agent Unavailable');
            break;
          case 'service_alert':
            this.alertService.showWarning(error.message, 'AI Service Alert');
            break;
          case 'maintenance':
            this.alertService.showMaintenance(error.message, error.startTime, error.estimatedDuration);
            break;
          default:
            this.alertService.showError(error.message || 'An AI service error occurred', 'AI Error');
        }
      }
    });
  }

  //#endregion

  //#region Inherited Methods (from original service)

  /**
   * Archives a conversation
   */
  public async archiveConversation(conversationId: string, type: ConversationType): Promise<boolean> {
    try {
      const connection = this.getConnectionForType(type);
      await (connection as ChatWebSocketConnection).archiveConversation(conversationId);
      
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
   * Sets the currently selected conversation
   */
  public setSelectedConversation(conversation: Conversation | null): void {
    this._selectedConversation$.next(conversation);
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
   * Cleanup method
   */
  public destroy(): void {
    this._dataSources.forEach(dataSource => {
      dataSource.dispose();
    });
    
    this._dataSources.clear();
    this._selectedConversation$.complete();
    this._conversationCounts$.complete();
    this._agentThinking$.complete();
    this._tokenLimits$.complete();
  }

  //#endregion
}