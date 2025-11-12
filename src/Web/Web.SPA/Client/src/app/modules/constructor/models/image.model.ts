import { GenerationSource } from './enums.model';

export interface GeneratedImage {
  imageId: string;
  configurationId: string;
  fileServiceUrl: string;
  fileName: string;
  generationPrompt: string;
  generationSource: GenerationSource;
  aspectRatio: string;
  generatedAt: Date;
  thumbnailUrl: string;
}

export interface GenerateImageRequest {
  configurationId: string;
  aspectRatio?: string;
}
