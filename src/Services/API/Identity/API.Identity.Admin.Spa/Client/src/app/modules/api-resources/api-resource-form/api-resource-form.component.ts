import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiResourcesService } from '../services/api-resources.service';
import { ApiResource } from '../models/api-resource.model';
import notify from 'devextreme/ui/notify';
import { confirm } from 'devextreme/ui/dialog';

@Component({
  selector: 'hbg-api-resource-form',
  templateUrl: './api-resource-form.component.html',
  styleUrls: ['./api-resource-form.component.scss'],
  providers: [ApiResourcesService]
})
export class ApiResourceFormComponent implements OnInit {
  resourceId: string | null = null;
  isEditMode = false;
  isLoading = false;
  isSaving = false;

  resource: ApiResource = this.getEmptyResource();

  // Multi-select items
  selectedScopes: string[] = [];
  selectedUserClaims: string[] = [];
  selectedSigningAlgorithms: string[] = [];

  // Available scopes (these should be fetched from API in production)
  availableScopes = [
    'api.read',
    'api.write',
    'api.delete',
    'api.admin',
    'api.full_access'
  ];

  // Common JWT signing algorithms
  availableSigningAlgorithms = [
    'RS256',
    'RS384',
    'RS512',
    'PS256',
    'PS384',
    'PS512',
    'ES256',
    'ES384',
    'ES512'
  ];

  // Common API claims
  availableUserClaims = [
    'sub',
    'name',
    'email',
    'role',
    'permissions',
    'department',
    'organization',
    'tenant_id'
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ApiResourcesService
  ) {}

  ngOnInit() {
    this.resourceId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.resourceId;

    if (this.isEditMode) {
      this.loadResource();
    }
  }

  async loadResource() {
    if (!this.resourceId) return;

    this.isLoading = true;
    try {
      const resource = await this.service.dataSource.store().byKey(this.resourceId);
      if (resource) {
        this.resource = resource;
        this.selectedScopes = resource.scopes || [];
        this.selectedUserClaims = resource.userClaims || [];
        this.selectedSigningAlgorithms = resource.allowedAccessTokenSigningAlgorithms || [];
      }
    } catch (error) {
      notify('Failed to load API resource', 'error', 3000);
      console.error('Error loading API resource:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async onSave() {
    // Validate required fields
    if (!this.resource.name) {
      notify('Name is required', 'warning', 3000);
      return;
    }

    this.isSaving = true;
    try {
      // Update arrays from UI
      this.resource.scopes = this.selectedScopes;
      this.resource.userClaims = this.selectedUserClaims;
      this.resource.allowedAccessTokenSigningAlgorithms = this.selectedSigningAlgorithms;

      if (this.isEditMode) {
        await this.service.dataSource.store().update(this.resource.id!, this.resource);
        notify('API resource updated successfully', 'success', 2000);
      } else {
        await this.service.dataSource.store().insert(this.resource);
        notify('API resource created successfully', 'success', 2000);
      }

      this.router.navigate(['/api-resources']);
    } catch (error) {
      notify(`Failed to ${this.isEditMode ? 'update' : 'create'} API resource`, 'error', 3000);
      console.error('Error saving API resource:', error);
    } finally {
      this.isSaving = false;
    }
  }

  async onCancel() {
    const result = await confirm(
      'Are you sure you want to cancel? Any unsaved changes will be lost.',
      'Confirm Cancel'
    );

    if (result) {
      this.router.navigate(['/api-resources']);
    }
  }

  private getEmptyResource(): ApiResource {
    return {
      name: '',
      displayName: '',
      description: '',
      enabled: true,
      showInDiscoveryDocument: true,
      scopes: [],
      userClaims: [],
      allowedAccessTokenSigningAlgorithms: [],
      nonEditable: false
    };
  }
}
