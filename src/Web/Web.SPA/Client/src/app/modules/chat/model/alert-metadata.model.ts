/**
 * Metadata for alert messages
 */
export interface AlertMetadata {
  /** Type of alert */
  alertType: 'info' | 'success' | 'warning' | 'error';
  
  /** Whether the alert persists or auto-hides */
  isPersistent: boolean;
  
  /** Auto-hide duration in milliseconds */
  autoHideDuration?: number;
  
  /** Whether this is a system-wide alert */
  isSystemWide: boolean;
  
  /** Optional action buttons */
  actions?: Array<{
    text: string;
    action: string;
    style?: 'default' | 'primary' | 'danger';
  }>;
}