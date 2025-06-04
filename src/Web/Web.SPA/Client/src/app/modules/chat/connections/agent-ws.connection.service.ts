import { Injectable, inject } from "@angular/core";
import { AuthService } from "@app/core/services/auth.service";
import { ConfigService } from "@app/core/services/config.service";
import { SignalRAction, WsConnection } from "@app/core/services/websocket/ws-connection";
import { BehaviorSubject, Observable } from "rxjs";

/**
 * Enhanced SignalR Hub connection for AI Agent functionality
 * 
 * This service manages a separate WebSocket connection to the Agent microservice
 * and provides specialized features for AI-powered conversations:
 * - Real-time AI message processing
 * - Conversation context management
 * - AI agent status and capabilities
 * - Specialized alert handling for AI interactions
 * - Token usage tracking and limits
 * 
 * The connection is separate from the main chat connection to allow for
 * different scaling and configuration of AI services vs regular chat.
 */
@Injectable()
export class AgentWebSocketConnection extends WsConnection {
  private config: ConfigService = inject(ConfigService);
  
  // Agent-specific observables
  private _agentStatus$ = new BehaviorSubject<{ agentId: string; status: string; capabilities: string[] } | null>(null);
  private _aiThinking$ = new BehaviorSubject<{ conversationId: string; isThinking: boolean } | null>(null);
  private _tokenUsage$ = new BehaviorSubject<{ conversationId: string; tokensUsed: number; tokensRemaining: number } | null>(null);
  private _agentErrors$ = new BehaviorSubject<any>(null);

  constructor(protected readonly auth: AuthService) {
    super(auth, auth.authStatus$);
  }

  

  // protected hubUrl: string = `${this.config.hbgagent}/hubs/agent`; // Different microservice
  protected hubUrl: string = `${this.config.hbgcontacts}/hubs/agent`; // Different microservice

  protected canConnect: () => boolean = () => false;

  //#region Public Observables

  /**
   * Observable for AI agent status changes
   */
  public get agentStatus$(): Observable<{ agentId: string; status: string; capabilities: string[] } | null> {
    return this._agentStatus$.asObservable();
  }

  /**
   * Observable for AI thinking indicators
   */
  public get aiThinking$(): Observable<{ conversationId: string; isThinking: boolean } | null> {
    return this._aiThinking$.asObservable();
  }

  /**
   * Observable for token usage tracking
   */
  public get tokenUsage$(): Observable<{ conversationId: string; tokensUsed: number; tokensRemaining: number } | null> {
    return this._tokenUsage$.asObservable();
  }

  /**
   * Observable for agent-specific errors
   */
  public get agentErrors$(): Observable<any> {
    return this._agentErrors$.asObservable();
  }

  //#endregion

  //#region Agent Methods

  /**
   * Loads conversations for AI agent
   */
  public async loadAgentConversations(loadOptions: any): Promise<any> {
    return await this.invoke('LoadAgentConversations', loadOptions);
  }

  /**
   * Creates a new AI agent conversation
   */
  public async createAgentConversation(topic: string, agentType?: string): Promise<any> {
    return await this.invoke('CreateAgentConversation', topic, agentType || 'general');
  }

  /**
   * Sends a message to the AI agent
   */
  public async sendMessageToAgent(
    conversationId: string, 
    message: string, 
    context?: any
  ): Promise<any> {
    return await this.invoke('SendMessageToAgent', conversationId, message, context);
  }

  /**
   * Requests AI agent capabilities
   */
  public async getAgentCapabilities(agentId?: string): Promise<any> {
    return await this.invoke('GetAgentCapabilities', agentId);
  }

  /**
   * Updates conversation context for better AI responses
   */
  public async updateConversationContext(
    conversationId: string, 
    context: any
  ): Promise<void> {
    return await this.invoke('UpdateConversationContext', conversationId, context);
  }

  /**
   * Requests a specific AI model or agent type
   */
  public async switchAgentModel(
    conversationId: string, 
    modelName: string
  ): Promise<any> {
    return await this.invoke('SwitchAgentModel', conversationId, modelName);
  }

  /**
   * Gets token usage for a conversation
   */
  public async getTokenUsage(conversationId: string): Promise<any> {
    return await this.invoke('GetTokenUsage', conversationId);
  }

  /**
   * Resets conversation context (clears AI memory)
   */
  public async resetConversationContext(conversationId: string): Promise<void> {
    return await this.invoke('ResetConversationContext', conversationId);
  }

  /**
   * Submits feedback about AI response quality
   */
  public async submitAIFeedback(
    messageId: string, 
    rating: number, 
    feedback?: string
  ): Promise<void> {
    return await this.invoke('SubmitAIFeedback', messageId, rating, feedback);
  }

  /**
   * Gets available AI models/agents
   */
  public async getAvailableAgents(): Promise<any[]> {
    return await this.invoke('GetAvailableAgents');
  }

  //#endregion

  //#region Event Handlers

  private readonly onAIMessageReceived = (message: any) => {
    console.log('AI message received:', message);
    // Handler will be picked up by SignalR data stores
  };

  private readonly onAIMessageProcessing = (conversationId: string) => {
    console.log('AI processing message for conversation:', conversationId);
    this._aiThinking$.next({ conversationId, isThinking: true });
  };

  private readonly onAIMessageProcessed = (conversationId: string) => {
    console.log('AI finished processing message for conversation:', conversationId);
    this._aiThinking$.next({ conversationId, isThinking: false });
  };

  private readonly onAIMessageError = (error: any) => {
    console.log('AI message error:', error);
    this._agentErrors$.next({ type: 'message_error', ...error });
  };

  private readonly onAgentStatusChanged = (agentId: string, status: string, capabilities: string[]) => {
    console.log('Agent status changed:', agentId, status, capabilities);
    this._agentStatus$.next({ agentId, status, capabilities });
  };

  private readonly onAgentCapabilitiesUpdated = (agentId: string, capabilities: string[]) => {
    console.log('Agent capabilities updated:', agentId, capabilities);
    const currentStatus = this._agentStatus$.value;
    if (currentStatus && currentStatus.agentId === agentId) {
      this._agentStatus$.next({ ...currentStatus, capabilities });
    }
  };

  private readonly onAgentThinkingStarted = (conversationId: string) => {
    console.log('Agent thinking started:', conversationId);
    this._aiThinking$.next({ conversationId, isThinking: true });
  };

  private readonly onAgentThinkingStopped = (conversationId: string) => {
    console.log('Agent thinking stopped:', conversationId);
    this._aiThinking$.next({ conversationId, isThinking: false });
  };

  private readonly onTokenUsageUpdated = (conversationId: string, tokensUsed: number, tokensRemaining: number) => {
    console.log('Token usage updated:', conversationId, tokensUsed, tokensRemaining);
    this._tokenUsage$.next({ conversationId, tokensUsed, tokensRemaining });
  };

  private readonly onTokenLimitReached = (conversationId: string) => {
    console.log('Token limit reached for conversation:', conversationId);
    this._agentErrors$.next({ 
      type: 'token_limit_reached', 
      conversationId,
      message: 'Token limit reached for this conversation. Please start a new conversation.' 
    });
  };

  private readonly onTokenLimitWarning = (conversationId: string, tokensRemaining: number) => {
    console.log('Token limit warning for conversation:', conversationId, tokensRemaining);
    this._agentErrors$.next({ 
      type: 'token_limit_warning', 
      conversationId,
      message: `Approaching token limit. ${tokensRemaining} tokens remaining.`,
      tokensRemaining 
    });
  };

  private readonly onAgentConversationCreated = (conversation: any) => {
    console.log('Agent conversation created:', conversation);
  };

  private readonly onAgentConversationUpdated = (conversation: any) => {
    console.log('Agent conversation updated:', conversation);
  };

  private readonly onConversationContextUpdated = (conversationId: string, context: any) => {
    console.log('Conversation context updated:', conversationId, context);
  };

  private readonly onAIServiceAlert = (alert: any) => {
    console.log('AI service alert:', alert);
    this._agentErrors$.next({ type: 'service_alert', ...alert });
  };

  private readonly onAgentUnavailableAlert = (agentId: string, reason: string) => {
    console.log('Agent unavailable:', agentId, reason);
    this._agentErrors$.next({ 
      type: 'agent_unavailable', 
      agentId, 
      message: `AI Agent ${agentId} is currently unavailable: ${reason}` 
    });
  };

  private readonly onAIServiceMaintenance = (message: string, startTime: Date, estimatedDuration: number) => {
    console.log('AI service maintenance:', message, startTime, estimatedDuration);
    this._agentErrors$.next({ 
      type: 'maintenance', 
      message, 
      startTime, 
      estimatedDuration 
    });
  };

  private readonly onModelUpdated = (modelName: string, version: string, capabilities: string[]) => {
    console.log('AI model updated:', modelName, version, capabilities);
    this._agentErrors$.next({ 
      type: 'model_updated', 
      message: `AI model ${modelName} has been updated to version ${version}`,
      modelName,
      version,
      capabilities 
    });
  };

  private readonly onReloadDataSource = (dataSourceName: string, parameters?: any) => {
    console.log('Reload data source (Agent):', dataSourceName, parameters);
  };

  private readonly onPushDataSourceChanges = (dataSourceName: string, changes: any[]) => {
    console.log('Push data source changes (Agent):', dataSourceName, changes);
  };

  //#endregion

  //#region Helper Methods

  /**
   * Check if an AI agent is currently thinking/processing
   */
  public isAgentThinking(conversationId: string): boolean {
    const thinking = this._aiThinking$.value;
    return thinking?.conversationId === conversationId && thinking.isThinking;
  }

  /**
   * Get current token usage for a conversation
   */
  public getConversationTokenUsage(conversationId: string): { tokensUsed: number; tokensRemaining: number } | null {
    const usage = this._tokenUsage$.value;
    return usage?.conversationId === conversationId ? 
      { tokensUsed: usage.tokensUsed, tokensRemaining: usage.tokensRemaining } : 
      null;
  }

  /**
   * Check if token limit is approaching for a conversation
   */
  public isTokenLimitApproaching(conversationId: string, threshold: number = 100): boolean {
    const usage = this.getConversationTokenUsage(conversationId);
    return usage ? usage.tokensRemaining <= threshold : false;
  }

  /**
   * Get current agent status
   */
  public getCurrentAgentStatus(): { agentId: string; status: string; capabilities: string[] } | null {
    return this._agentStatus$.value;
  }

  //#endregion

  public override actions: SignalRAction[] = [
    // Message events (AI-specific)
    { name: "AIMessageReceived", handler: this.onAIMessageReceived },
    { name: "AIMessageProcessing", handler: this.onAIMessageProcessing },
    { name: "AIMessageProcessed", handler: this.onAIMessageProcessed },
    { name: "AIMessageError", handler: this.onAIMessageError },
    
    // Agent status events
    { name: "AgentStatusChanged", handler: this.onAgentStatusChanged },
    { name: "AgentCapabilitiesUpdated", handler: this.onAgentCapabilitiesUpdated },
    { name: "AgentThinkingStarted", handler: this.onAgentThinkingStarted },
    { name: "AgentThinkingStopped", handler: this.onAgentThinkingStopped },
    
    // Token and usage events
    { name: "TokenUsageUpdated", handler: this.onTokenUsageUpdated },
    { name: "TokenLimitReached", handler: this.onTokenLimitReached },
    { name: "TokenLimitWarning", handler: this.onTokenLimitWarning },
    
    // Conversation events (Agent-specific)
    { name: "AgentConversationCreated", handler: this.onAgentConversationCreated },
    { name: "AgentConversationUpdated", handler: this.onAgentConversationUpdated },
    { name: "ConversationContextUpdated", handler: this.onConversationContextUpdated },
    
    // Agent-specific alerts
    { name: "AIServiceAlert", handler: this.onAIServiceAlert },
    { name: "AgentUnavailableAlert", handler: this.onAgentUnavailableAlert },
    
    // System events
    { name: "AIServiceMaintenance", handler: this.onAIServiceMaintenance },
    { name: "ModelUpdated", handler: this.onModelUpdated },
    
    // Standard DataSource events
    { name: "ReloadDataSource", handler: this.onReloadDataSource },
    { name: "PushDataSourceChanges", handler: this.onPushDataSourceChanges }
  ];

  //#region Cleanup

  /**
   * Override disconnect to clean up agent-specific state
   */
  public override disconnect(): void {
    // Reset all agent-specific observables
    this._agentStatus$.next(null);
    this._aiThinking$.next(null);
    this._tokenUsage$.next(null);
    this._agentErrors$.next(null);
    
    super.disconnect();
  }

  //#endregion
}