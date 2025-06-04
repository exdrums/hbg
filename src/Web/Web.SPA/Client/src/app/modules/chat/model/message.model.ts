import { MessageError } from "./message-error.model";
import { MessageMetadata } from "./message-metadata.model";
import { MessageType } from "./message-type.enum";

/**
 * Core message domain model
 * Represents a single message within a conversation
 */
export interface Message {
  /** Unique identifier for the message */
  messageId: string;
  
  /** The conversation this message belongs to */
  conversationId: string;
  
  /** User ID of the message sender */
  senderUserId: string;
  
  /** The message content (text, or description for other types) */
  content: string;
  
  /** Type of message determining rendering behavior */
  type: MessageType;
  
  /** When the message was sent */
  sentAt: Date;
  
  /** When the message was last edited (null if never edited) */
  editedAt?: Date;
  
  /** Whether the message has been deleted */
  isDeleted: boolean;
  
  /** List of user IDs who have read this message */
  readByUserIds: string[];
  
  /** If this is a reply, the ID of the message being replied to */
  replyToMessageId?: string;
  
  /** Additional metadata for special message types */
  metadata?: MessageMetadata;
  
  /** Temporary flag for optimistic updates */
  isPending?: boolean;
  
  /** Error state for failed messages */
  error?: MessageError;
}