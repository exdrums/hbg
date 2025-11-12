import { JewelryType } from './enums.model';

export interface Project {
  projectId: string;
  userId: string;
  name: string;
  description: string;
  jewelryType: JewelryType;
  createdAt: Date;
  updatedAt: Date;
  isActive: boolean;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
  jewelryType: JewelryType;
}

export interface UpdateProjectRequest {
  name: string;
  description: string;
  isActive: boolean;
}
