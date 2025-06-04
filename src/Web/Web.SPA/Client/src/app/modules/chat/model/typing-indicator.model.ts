/**
 * Typing indicator information
 */
export interface TypingIndicator {
  /** The conversation where typing is happening */
  conversationId: string;
  
  /** The user who is typing */
  userId: string;
  
  /** When the typing started */
  startedAt: Date;
  
  /** Whether the user is currently typing */
  isTyping: boolean;
}