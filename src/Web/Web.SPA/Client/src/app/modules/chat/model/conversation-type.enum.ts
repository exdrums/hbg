/**
 * Enumeration of conversation types supported by the system
 * Each type has different behavior and routing requirements
 */
export enum ConversationType {
  /** Direct messages and group chats with other users */
  Contacts = 'Contacts',
  /** Support ticket conversations with support agents */
  Support = 'Support',
  /** AI-powered agent conversations */
  Agent = 'Agent'
}