import { ConversationType } from "./conversation-type.enum";
import { ConversationVisibility } from "./conversation-visibility.enum";
import { Conversation } from "./conversation.model";

/**
 * Factory methods for creating conversations
 * Encapsulates the business logic for conversation creation
 */
export class ConversationFactory {
  /**
   * Creates a new direct conversation between two users
   * 
   * @param creatorUserId - The ID of the user creating the conversation
   * @param otherUserId - The ID of the other participant
   * @returns A new direct conversation instance
   */
  static createDirectConversation(
    creatorUserId: string,
    otherUserId: string
  ): Omit<Conversation, 'conversationId'> {
    return {
      type: ConversationVisibility.Direct,
      participantIds: [creatorUserId, otherUserId],
      createdByUserId: creatorUserId,
      createdAt: new Date(),
      isActive: true,
      unreadCount: 0,
      category: ConversationType.Contacts
    };
  }

  /**
   * Creates a new group conversation with multiple participants
   * 
   * @param creatorUserId - The ID of the user creating the conversation
   * @param title - The title for the group conversation
   * @param participantIds - Array of participant user IDs
   * @param category - The category of conversation (default: Contacts)
   * @returns A new group conversation instance
   */
  static createGroupConversation(
    creatorUserId: string,
    title: string,
    participantIds: string[],
    category: ConversationType = ConversationType.Contacts
  ): Omit<Conversation, 'conversationId'> {
    // Ensure creator is included in participants
    const uniqueParticipants = Array.from(new Set([...participantIds, creatorUserId]));
    
    return {
      title,
      type: ConversationVisibility.Group,
      participantIds: uniqueParticipants,
      createdByUserId: creatorUserId,
      createdAt: new Date(),
      isActive: true,
      unreadCount: 0,
      category
    };
  }

  /**
   * Creates a new support ticket conversation
   * 
   * @param userId - The ID of the user creating the ticket
   * @param subject - The subject/title of the support ticket
   * @param priority - The priority level of the ticket
   * @returns A new support conversation instance
   */
  static createSupportConversation(
    userId: string,
    subject: string,
    priority: 'low' | 'medium' | 'high' | 'urgent' = 'medium'
  ): Omit<Conversation, 'conversationId'> {
    return {
      title: `Support: ${subject}`,
      type: ConversationVisibility.Group,
      participantIds: [userId],
      createdByUserId: userId,
      createdAt: new Date(),
      isActive: true,
      unreadCount: 0,
      category: ConversationType.Support,
      metadata: {
        support: {
          status: 'open',
          priority,
          ticketNumber: `TICKET-${Date.now()}`,
          category: 'general'
        }
      }
    };
  }

  /**
   * Creates a new AI agent conversation
   * 
   * @param userId - The ID of the user starting the conversation
   * @param topic - The topic or title for the AI conversation
   * @param agentType - The type of AI agent to use
   * @returns A new agent conversation instance
   */
  static createAgentConversation(
    userId: string,
    topic: string,
    agentType: string = 'general'
  ): Omit<Conversation, 'conversationId'> {
    return {
      title: `AI: ${topic}`,
      type: ConversationVisibility.Direct,
      participantIds: [userId, `agent_${agentType}`],
      createdByUserId: userId,
      createdAt: new Date(),
      isActive: true,
      unreadCount: 0,
      category: ConversationType.Agent,
      metadata: {
        agent: {
          agentType,
          tokenUsage: {
            used: 0,
            remaining: 1000,
            limit: 1000
          },
          capabilities: ['chat', 'context-aware', 'multi-turn']
        }
      }
    };
  }
}

/**
 * Type guard to check if a conversation is a support conversation
 */
export function isSupportConversation(conversation: Conversation): boolean {
  return conversation.category === ConversationType.Support;
}

/**
 * Type guard to check if a conversation is an agent conversation
 */
export function isAgentConversation(conversation: Conversation): boolean {
  return conversation.category === ConversationType.Agent;
}

/**
 * Type guard to check if a conversation is a direct message
 */
export function isDirectConversation(conversation: Conversation): boolean {
  return conversation.type === ConversationVisibility.Direct && 
         conversation.category === ConversationType.Contacts;
}

/**
 * Helper to get display title for a conversation
 * For direct messages, this would typically show the other user's name
 */
export function getConversationDisplayTitle(
  conversation: Conversation,
  currentUserId: string,
  userNames: Map<string, string>
): string {
  if (!conversation) return '';
  
  // For direct conversations, show the other participant's name
  if (conversation.type === ConversationVisibility.Direct && 
      conversation.category === ConversationType.Contacts) {
    const otherUserId = conversation.participantIds.find(id => id !== currentUserId);
    return otherUserId ? userNames.get(otherUserId) || 'Unknown User' : 'Direct Message';
  }
  
  // For other types, use the title
  return conversation.title || 'Untitled Conversation';
}