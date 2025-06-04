import { AgentMetadata } from "./agent-metadata.model";
import { SupportMetadata } from "./support-metadata.model";

/**
 * Metadata for conversations with type-specific properties
 */
export interface ConversationMetadata {
  /** Support-specific metadata */
  support?: SupportMetadata;
  
  /** AI Agent-specific metadata */
  agent?: AgentMetadata;
  
  /** Custom key-value pairs for extensibility */
  custom?: Record<string, any>;
}