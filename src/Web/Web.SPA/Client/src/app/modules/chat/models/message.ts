import { User } from "./user";

/**
 * Message model representing a chat message
 */
export interface Message {
  id: string;
  text: string;
  timestamp: Date;
  author: User;
  isEdited?: boolean;
  isBeingRegenerated?: boolean;
  parentMessageId?: string;
}
