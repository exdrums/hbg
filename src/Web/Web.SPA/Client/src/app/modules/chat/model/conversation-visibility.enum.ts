/**
 * Enumeration of conversation visibility types
 * Determines who can see and participate in conversations
 */
export enum ConversationVisibility {
  /** One-on-one conversation between two users */
  Direct = 'Direct',
  /** Group conversation with multiple participants */
  Group = 'Group',
  /** System-generated conversations for announcements */
  System = 'System'
}