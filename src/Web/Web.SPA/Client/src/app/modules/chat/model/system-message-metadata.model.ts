/**
 * Metadata for system messages
 */
export interface SystemMessageMetadata {
  /** Type of system event */
  eventType: 'user_joined' | 'user_left' | 'user_added' | 'user_removed' | 
             'title_changed' | 'settings_changed' | 'conversation_created' |
             'conversation_archived' | 'ticket_closed' | 'agent_assigned';
  
  /** User IDs involved in the event */
  affectedUserIds?: string[];
  
  /** Previous value (for change events) */
  previousValue?: any;
  
  /** New value (for change events) */
  newValue?: any;
}