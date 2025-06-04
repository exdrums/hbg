import { ConversationMetadata } from "./conversation-metadata.model";
import { ConversationType } from "./conversation-type.enum";
import { ConversationVisibility } from "./conversation-visibility.enum";

/**
 * Core conversation domain model
 * Represents a chat conversation with all its properties and metadata
 */
export interface Conversation {
  /** Unique identifier for the conversation */
  conversationId: string;
  
  /** Display title for the conversation (optional for direct messages) */
  title?: string;
  
  /** Type of conversation determining visibility and behavior */
  type: ConversationVisibility;
  
  /** List of participant user IDs in this conversation */
  participantIds: string[];
  
  /** User ID of the conversation creator */
  createdByUserId: string;
  
  /** Timestamp when the conversation was created */
  createdAt: Date;
  
  /** Timestamp of the last message in the conversation */
  lastMessageAt?: Date;
  
  /** Whether the conversation is active or archived */
  isActive: boolean;
  
  /** Preview text of the last message for list display */
  lastMessagePreview?: string;
  
  /** Number of unread messages for the current user */
  unreadCount: number;
  
  /** Category of conversation (Contacts, Support, Agent) */
  category?: ConversationType;
  
  /** Additional metadata for specific conversation types */
  metadata?: ConversationMetadata;
}