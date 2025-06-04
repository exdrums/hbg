/**
 * Error information for failed messages
 */
export interface MessageError {
  /** Error code for programmatic handling */
  code: string;
  
  /** Human-readable error message */
  message: string;
  
  /** Whether the message can be retried */
  canRetry: boolean;
  
  /** Number of retry attempts made */
  retryCount?: number;
}