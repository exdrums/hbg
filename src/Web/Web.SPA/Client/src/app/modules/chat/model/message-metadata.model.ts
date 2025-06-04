import { AlertMetadata } from "./alert-metadata.model";
import { FileMetadata } from "./file-metadata.model";
import { LocationMetadata } from "./location-metadata.model";
import { SystemMessageMetadata } from "./system-message-metadata.model";

/**
 * Metadata for messages with type-specific properties
 */
export interface MessageMetadata {
  /** File/media information */
  file?: FileMetadata;
  
  /** Location information */
  location?: LocationMetadata;
  
  /** Alert-specific metadata */
  alert?: AlertMetadata;
  
  /** System message metadata */
  system?: SystemMessageMetadata;
  
  /** Custom metadata for extensibility */
  custom?: Record<string, any>;
}