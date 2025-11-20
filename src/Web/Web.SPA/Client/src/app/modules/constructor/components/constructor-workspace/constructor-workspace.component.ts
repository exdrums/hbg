import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import notify from 'devextreme/ui/notify';
import { devices } from 'devextreme/core/devices';
import { ConstructorService } from '../../services/constructor.service';
import { ConstructorHubService } from '../../services/constructor-hub.service';
import { Project, GeneratedImage, FormData } from '../../models';

@Component({
  selector: 'app-constructor-workspace',
  templateUrl: './constructor-workspace.component.html',
  styleUrls: ['./constructor-workspace.component.scss']
})
export class ConstructorWorkspaceComponent implements OnInit, OnDestroy {
  currentProject: Project | null = null;
  generatedImages: GeneratedImage[] = [];
  isGenerating = false;
  currentConfiguration: FormData = {};

  // Responsive layout
  isMobile = false;
  isViewPopupVisible = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private constructorService: ConstructorService,
    private hubService: ConstructorHubService
  ) {
    this.isMobile = devices.current().deviceType === 'phone' || devices.current().deviceType === 'tablet';
  }

  async ngOnInit() {
    // Connect to SignalR hub
    try {
      await this.hubService.connect();

      // Subscribe to real-time updates
      this.hubService.onImageUpdate$
        .pipe(takeUntil(this.destroy$))
        .subscribe(update => {
          if (update && update.imageUrl) {
            notify('Image generated successfully!', 'success', 3000);
            this.loadProjectImages();
          }
        });

      this.hubService.onError$
        .pipe(takeUntil(this.destroy$))
        .subscribe(error => {
          if (error) {
            notify(error.message, 'error', 3000);
            this.isGenerating = false;
          }
        });

      this.hubService.onGenerationStarted$
        .pipe(takeUntil(this.destroy$))
        .subscribe(started => {
          this.isGenerating = started;
        });

    } catch (error) {
      console.error('Failed to connect to SignalR hub:', error);
    }

    // Load project from route
    const projectId = this.route.snapshot.params['id'];
    if (projectId) {
      await this.loadProject(projectId);
    } else {
      this.router.navigate(['/constructor']);
    }
  }

  async loadProject(projectId: string) {
    try {
      this.currentProject = await this.constructorService.getProject(projectId);
      await this.loadProjectImages();
    } catch (error) {
      console.error('Error loading project:', error);
      notify('Failed to load project', 'error', 3000);
      this.router.navigate(['/constructor']);
    }
  }

  async loadProjectImages() {
    if (!this.currentProject) return;

    try {
      this.generatedImages = await this.constructorService.getProjectImages(
        this.currentProject.projectId
      );
    } catch (error) {
      console.error('Error loading images:', error);
    }
  }

  handleConfigurationChange(formData: FormData) {
    this.currentConfiguration = formData;
  }

  async generateImage(formData: FormData) {
    if (!this.currentProject) {
      notify('No project selected', 'error', 3000);
      return;
    }

    this.isGenerating = true;

    try {
      // Save configuration
      const config = await this.constructorService.saveConfiguration({
        projectId: this.currentProject.projectId,
        configurationName: `Config ${new Date().toLocaleString()}`,
        formData: formData
      });

      // Generate image
      const image = await this.constructorService.generateImage({
        configurationId: config.configurationId,
        aspectRatio: formData.aspectRatio || '1:1'
      });

      // Add image to gallery
      this.generatedImages = [image, ...this.generatedImages];
      notify('Image generated successfully!', 'success', 3000);

    } catch (error: any) {
      console.error('Error generating image:', error);
      notify(error.error?.message || 'Failed to generate image', 'error', 5000);
    } finally {
      this.isGenerating = false;
    }
  }

  // Mobile view popup controls
  openViewPopup() {
    this.isViewPopupVisible = true;
  }

  closeViewPopup() {
    this.isViewPopupVisible = false;
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.hubService.disconnect();
  }
}
