/**
 * Enumeration of supported message types
 * Each type has different rendering and behavior in the UI
 */
export enum MessageType {
  /** Standard text message */
  Text = 'Text',
  /** Image attachment */
  Image = 'Image',
  /** File attachment */
  File = 'File',
  /** System-generated message (user joined, left, etc.) */
  System = 'System',
  /** Audio message or voice note */
  Audio = 'Audio',
  /** Video message */
  Video = 'Video',
  /** Location sharing */
  Location = 'Location',
  /** Non-persistent alert message */
  Alert = 'Alert'
}