import { AlertMetadata } from "./alert-metadata.model";
import { FileMetadata } from "./file-metadata.model";
import { MessageType } from "./message-type.enum";
import { Message } from "./message.model";
import { SystemMessageMetadata } from "./system-message-metadata.model";

/**
 * Factory methods for creating messages
 * Encapsulates the business logic for message creation
 */
export class MessageFactory {
  /**
   * Creates a new text message
   * 
   * @param conversationId - The target conversation
   * @param senderUserId - The sender's user ID
   * @param content - The message text content
   * @param replyToMessageId - Optional message being replied to
   * @returns A new text message instance
   */
  static createTextMessage(
    conversationId: string,
    senderUserId: string,
    content: string,
    replyToMessageId?: string
  ): Omit<Message, 'messageId'> {
    return {
      conversationId,
      senderUserId,
      content: content.trim(),
      type: MessageType.Text,
      sentAt: new Date(),
      isDeleted: false,
      readByUserIds: [senderUserId], // Sender has read their own message
      replyToMessageId,
      isPending: true
    };
  }

  /**
   * Creates a system message for events
   * 
   * @param conversationId - The target conversation
   * @param content - The system message content
   * @param eventType - The type of system event
   * @param metadata - Additional event metadata
   * @returns A new system message instance
   */
  static createSystemMessage(
    conversationId: string,
    content: string,
    eventType: SystemMessageMetadata['eventType'],
    metadata?: Partial<SystemMessageMetadata>
  ): Omit<Message, 'messageId'> {
    return {
      conversationId,
      senderUserId: 'SYSTEM',
      content,
      type: MessageType.System,
      sentAt: new Date(),
      isDeleted: false,
      readByUserIds: [],
      metadata: {
        system: {
          eventType,
          ...metadata
        }
      }
    };
  }

  /**
   * Creates an alert message (non-persistent)
   * 
   * @param conversationId - The target conversation (or empty for global)
   * @param content - The alert message content
   * @param alertType - The type of alert
   * @param options - Additional alert options
   * @returns A new alert message instance
   */
  static createAlertMessage(
    conversationId: string,
    content: string,
    alertType: AlertMetadata['alertType'] = 'info',
    options?: Partial<AlertMetadata>
  ): Message {
    return {
      messageId: `alert_${Date.now()}_${Math.random()}`,
      conversationId,
      senderUserId: 'SYSTEM',
      content,
      type: MessageType.Alert,
      sentAt: new Date(),
      isDeleted: false,
      readByUserIds: [],
      metadata: {
        alert: {
          alertType,
          isPersistent: false,
          isSystemWide: false,
          autoHideDuration: 5000,
          ...options
        }
      }
    };
  }

  /**
   * Creates a file message
   * 
   * @param conversationId - The target conversation
   * @param senderUserId - The sender's user ID
   * @param file - File metadata
   * @returns A new file message instance
   */
  static createFileMessage(
    conversationId: string,
    senderUserId: string,
    file: FileMetadata
  ): Omit<Message, 'messageId'> {
    return {
      conversationId,
      senderUserId,
      content: file.fileName,
      type: MessageType.File,
      sentAt: new Date(),
      isDeleted: false,
      readByUserIds: [senderUserId],
      metadata: { file },
      isPending: true
    };
  }
}

/**
 * Helper functions for message operations
 */

/**
 * Checks if a message is from the current user
 */
export function isOwnMessage(message: Message, currentUserId: string): boolean {
  return message.senderUserId === currentUserId;
}

/**
 * Checks if a message has been read by a specific user
 */
export function isMessageReadByUser(message: Message, userId: string): boolean {
  return message.readByUserIds.includes(userId);
}

/**
 * Checks if a message can be edited by a user
 * Business rule: Only own messages can be edited within 15 minutes
 */
export function canEditMessage(message: Message, userId: string): boolean {
  if (message.isDeleted) return false;
  if (message.type === MessageType.System) return false;
  if (message.type === MessageType.Alert) return false;
  if (message.senderUserId !== userId) return false;
  
  const fifteenMinutes = 15 * 60 * 1000;
  const messageAge = Date.now() - new Date(message.sentAt).getTime();
  
  return messageAge < fifteenMinutes;
}

/**
 * Checks if a message can be deleted by a user
 * Business rule: Only own messages can be deleted
 */
export function canDeleteMessage(message: Message, userId: string): boolean {
  if (message.isDeleted) return false;
  if (message.type === MessageType.System) return false;
  if (message.type === MessageType.Alert) return false;
  
  return message.senderUserId === userId;
}

/**
 * Groups messages by date for display
 */
export function groupMessagesByDate(messages: Message[]): Map<string, Message[]> {
  const groups = new Map<string, Message[]>();
  
  messages.forEach(message => {
    const date = new Date(message.sentAt);
    const dateKey = date.toDateString();
    
    if (!groups.has(dateKey)) {
      groups.set(dateKey, []);
    }
    
    groups.get(dateKey)!.push(message);
  });
  
  return groups;
}

/**
 * Formats a message date for display
 */
export function formatMessageDate(date: Date): string {
  const now = new Date();
  const messageDate = new Date(date);
  const diffInDays = Math.floor((now.getTime() - messageDate.getTime()) / (1000 * 60 * 60 * 24));
  
  if (diffInDays === 0) {
    return 'Today';
  } else if (diffInDays === 1) {
    return 'Yesterday';
  } else if (diffInDays < 7) {
    return messageDate.toLocaleDateString('en-US', { weekday: 'long' });
  } else {
    return messageDate.toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric', 
      year: messageDate.getFullYear() !== now.getFullYear() ? 'numeric' : undefined 
    });
  }
}