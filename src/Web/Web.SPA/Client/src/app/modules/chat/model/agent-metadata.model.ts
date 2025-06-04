/**
 * Metadata specific to AI agent conversations
 */
export interface AgentMetadata {
  /** Type of AI agent being used */
  agentType: string;
  
  /** AI model name/version */
  modelName?: string;
  
  /** Token usage tracking */
  tokenUsage?: {
    used: number;
    remaining: number;
    limit: number;
  };
  
  /** Conversation context for AI continuity */
  context?: any;
  
  /** AI agent capabilities */
  capabilities?: string[];
  
  /** Custom AI parameters */
  parameters?: Record<string, any>;
}