/**
 * Metadata specific to support conversations
 */
export interface SupportMetadata {
  /** Current ticket status */
  status: 'open' | 'closed' | 'pending' | 'resolved';
  
  /** Priority level of the support ticket */
  priority: 'low' | 'medium' | 'high' | 'urgent';
  
  /** Assigned support agent user ID */
  assignedAgentId?: string;
  
  /** Ticket number for reference */
  ticketNumber?: string;
  
  /** Category of support issue */
  category?: string;
  
  /** Resolution notes */
  resolution?: string;
}