/**
 * Participant information for a conversation
 * Represents a user's membership and state within a conversation
 */
export interface ConversationParticipant {
  /** The conversation this participation refers to */
  conversationId: string;
  
  /** The participant's user ID */
  userId: string;
  
  /** When this user joined the conversation */
  joinedAt: Date;
  
  /** When this user left the conversation (null if still active) */
  leftAt?: Date;
  
  /** User's role in the conversation */
  role: 'member' | 'admin' | 'moderator' | 'observer';
  
  /** Last time this user viewed the conversation */
  lastReadAt?: Date;
  
  /** User's notification preferences for this conversation */
  notificationPreference: 'all' | 'mentions' | 'none';
  
  /** Whether the user has muted this conversation */
  isMuted: boolean;
  
  /** Custom nickname for this user in this conversation */
  nickname?: string;
}