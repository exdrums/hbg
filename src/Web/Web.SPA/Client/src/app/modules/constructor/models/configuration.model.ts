export interface Configuration {
  configurationId: string;
  projectId: string;
  configurationName: string;
  formData: { [key: string]: any };
  generatedPrompt: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateConfigurationRequest {
  projectId: string;
  configurationName: string;
  formData: { [key: string]: any };
}

export interface FormData {
  jewelryType?: number;
  material?: string;
  gemstone?: string;
  style?: string;
  finish?: string;
  aspectRatio?: string;
  notes?: string;
}
