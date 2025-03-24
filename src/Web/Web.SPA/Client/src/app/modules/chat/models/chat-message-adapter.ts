import { Message } from "./message";

/**
 * DevExtreme Chat component message format adapter
 * Converts between backend Message model and DevExtreme Chat component format
 */
export class ChatMessageAdapter {
  /**
   * Convert from backend Message to DevExtreme Chat message format
   */
  static toDxChatMessage(message: Message): any {
    return {
      id: message.id,
      text: message.text,
      timestamp: new Date(message.timestamp),
      author: {
        id: message.author.id,
        name: message.author.name,
        avatarUrl: message.author.avatarUrl,
        avatarAlt: message.author.avatarAlt,
      },
      // Add any DevExtreme Chat-specific properties here
    };
  }

  /**
   * Convert from DevExtreme Chat message format to backend Message
   */
  static fromDxChatMessage(dxMessage: any): Partial<Message> {
    return {
      text: dxMessage.text,
      // Only include fields needed for sending a new message
    };
  }
}
