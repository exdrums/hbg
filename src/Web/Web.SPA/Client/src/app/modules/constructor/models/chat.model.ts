export interface ChatMessage {
  interactionId: string;
  projectId: string;
  userMessage: string;
  assistantResponse: string;
  createdAt: Date;
}

export interface ImageUpdateEvent {
  imageId: string;
  configurationId: string;
  imageUrl: string;
  thumbnailUrl: string;
  generatedAt: Date;
  message: string;
}

export interface ChatResponseEvent {
  message: string;
  userMessage: string;
  timestamp: Date;
}

export interface ErrorEvent {
  message: string;
}
