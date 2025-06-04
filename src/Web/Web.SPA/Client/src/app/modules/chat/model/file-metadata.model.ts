/**
 * Metadata for file and media messages
 */
export interface FileMetadata {
  /** Original file name */
  fileName: string;
  
  /** File size in bytes */
  fileSize: number;
  
  /** URL to download/access the file */
  fileUrl: string;
  
  /** MIME type of the file */
  mimeType: string;
  
  /** Thumbnail URL for images/videos */
  thumbnailUrl?: string;
  
  /** Duration in seconds for audio/video */
  duration?: number;
  
  /** Dimensions for images/videos */
  dimensions?: {
    width: number;
    height: number;
  };
}