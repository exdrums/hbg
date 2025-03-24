import { ConversationType } from "./conversation-type";
import { User } from "./user";

/**
 * Conversation model representing a chat conversation
 */
export interface Conversation {
  id: string;
  title: string;
  type: ConversationType;
  participants: User[];
  lastMessageAt: Date;
  isArchived: boolean;
}
