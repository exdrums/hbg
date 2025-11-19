import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import notify from 'devextreme/ui/notify';
import { confirm } from 'devextreme/ui/dialog';
import { ConstructorService } from '../../services/constructor.service';
import { Project, JewelryType, JewelryTypeLabels } from '../../models';
import { ClickEvent } from 'devextreme/ui/button';

@Component({
  selector: 'app-project-list',
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss']
})
export class ProjectListComponent implements OnInit {
  projects: Project[] = [];
  loading = false;
  showNewProjectPopup = false;

  newProject = {
    name: '',
    description: '',
    jewelryType: JewelryType.Ring
  };

  jewelryTypes = Object.keys(JewelryType)
    .filter(key => !isNaN(Number(JewelryType[key as any])))
    .map(key => ({
      value: JewelryType[key as any],
      text: key
    }));

  constructor(
    private constructorService: ConstructorService,
    private router: Router
  ) {}

  async ngOnInit() {
    await this.loadProjects();
  }

  async loadProjects() {
    this.loading = true;
    try {
      this.projects = await this.constructorService.getProjects();
    } catch (error) {
      console.error('Error loading projects:', error);
      notify('Failed to load projects', 'error', 3000);
    } finally {
      this.loading = false;
    }
  }

  openProject(project: Project) {
    this.router.navigate(['/constructor', project.projectId]);
  }

  async createProject() {
    if (!this.newProject.name) {
      notify('Please enter a project name', 'warning', 3000);
      return;
    }

    try {
      const project = await this.constructorService.createProject(this.newProject);
      notify('Project created successfully!', 'success', 3000);
      this.showNewProjectPopup = false;
      this.router.navigate(['/constructor', project.projectId]);
    } catch (error) {
      console.error('Error creating project:', error);
      notify('Failed to create project', 'error', 3000);
    }
  }

  async deleteProject(project: Project, event: ClickEvent) {
    // event.stopPropagation();

    const result = await confirm(
      `Are you sure you want to delete "${project.name}"?`,
      'Confirm Deletion'
    );

    if (result) {
      try {
        await this.constructorService.deleteProject(project.projectId);
        notify('Project deleted', 'success', 3000);
        await this.loadProjects();
      } catch (error) {
        console.error('Error deleting project:', error);
        notify('Failed to delete project', 'error', 3000);
      }
    }
  }

  getJewelryTypeLabel(type: JewelryType): string {
    return JewelryTypeLabels[type] || 'Unknown';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }

  onNewProjectPopupHidden() {
    this.newProject = {
      name: '',
      description: '',
      jewelryType: JewelryType.Ring
    };
  }
}
