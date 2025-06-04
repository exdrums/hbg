/**
 * Metadata for location messages
 */
export interface LocationMetadata {
  /** Latitude coordinate */
  latitude: number;
  
  /** Longitude coordinate */
  longitude: number;
  
  /** Human-readable address or place name */
  address?: string;
  
  /** Map preview image URL */
  mapPreviewUrl?: string;
}