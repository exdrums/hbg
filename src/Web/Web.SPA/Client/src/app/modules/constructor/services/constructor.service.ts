import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { ConfigService } from '@app/core/services/config.service';
import {
  Project,
  CreateProjectRequest,
  UpdateProjectRequest,
  Configuration,
  CreateConfigurationRequest,
  GeneratedImage,
  GenerateImageRequest
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class ConstructorService {
  private baseUrl: string;

  constructor(
    private http: HttpClient,
    private config: ConfigService
  ) {
    this.baseUrl = this.config.appSettings.HBGCONSTRUCTOR;
  }

  // Projects
  async getProjects(): Promise<Project[]> {
    return firstValueFrom(
      this.http.get<Project[]>(`${this.baseUrl}/api/projects`)
    );
  }

  async getProject(projectId: string): Promise<Project> {
    return firstValueFrom(
      this.http.get<Project>(`${this.baseUrl}/api/projects/${projectId}`)
    );
  }

  async createProject(request: CreateProjectRequest): Promise<Project> {
    return firstValueFrom(
      this.http.post<Project>(`${this.baseUrl}/api/projects`, request)
    );
  }

  async updateProject(projectId: string, request: UpdateProjectRequest): Promise<Project> {
    return firstValueFrom(
      this.http.put<Project>(`${this.baseUrl}/api/projects/${projectId}`, request)
    );
  }

  async deleteProject(projectId: string): Promise<void> {
    return firstValueFrom(
      this.http.delete<void>(`${this.baseUrl}/api/projects/${projectId}`)
    );
  }

  async getProjectImages(projectId: string): Promise<GeneratedImage[]> {
    return firstValueFrom(
      this.http.get<GeneratedImage[]>(`${this.baseUrl}/api/projects/${projectId}/images`)
    );
  }

  // Configurations
  async getConfiguration(configurationId: string): Promise<Configuration> {
    return firstValueFrom(
      this.http.get<Configuration>(`${this.baseUrl}/api/configurations/${configurationId}`)
    );
  }

  async saveConfiguration(request: CreateConfigurationRequest): Promise<Configuration> {
    return firstValueFrom(
      this.http.post<Configuration>(`${this.baseUrl}/api/configurations`, request)
    );
  }

  async generateImage(request: GenerateImageRequest): Promise<GeneratedImage> {
    return firstValueFrom(
      this.http.post<GeneratedImage>(
        `${this.baseUrl}/api/configurations/${request.configurationId}/generate`,
        { aspectRatio: request.aspectRatio || '1:1' }
      )
    );
  }

  // Images
  async getImage(imageId: string): Promise<GeneratedImage> {
    return firstValueFrom(
      this.http.get<GeneratedImage>(`${this.baseUrl}/api/images/${imageId}`)
    );
  }

  async deleteImage(imageId: string): Promise<void> {
    return firstValueFrom(
      this.http.delete<void>(`${this.baseUrl}/api/images/${imageId}`)
    );
  }
}
