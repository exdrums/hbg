/**
 * Read receipt information
 */
export interface MessageReadReceipt {
  /** The message that was read */
  messageId: string;
  
  /** The user who read the message */
  userId: string;
  
  /** When the message was read */
  readAt: Date;
}