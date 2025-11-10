import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { IdentityResourcesService } from '../services/identity-resources.service';
import { IdentityResource } from '../models/identity-resource.model';
import notify from 'devextreme/ui/notify';
import { confirm } from 'devextreme/ui/dialog';

@Component({
  selector: 'hbg-identity-resource-form',
  templateUrl: './identity-resource-form.component.html',
  styleUrls: ['./identity-resource-form.component.scss'],
  providers: [IdentityResourcesService]
})
export class IdentityResourceFormComponent implements OnInit {
  resourceId: string | null = null;
  isEditMode = false;
  isLoading = false;
  isSaving = false;

  resource: IdentityResource = this.getEmptyResource();

  // Multi-select items
  selectedUserClaims: string[] = [];

  // Common OIDC claims
  availableUserClaims = [
    'sub',
    'name',
    'given_name',
    'family_name',
    'middle_name',
    'nickname',
    'preferred_username',
    'profile',
    'picture',
    'website',
    'email',
    'email_verified',
    'gender',
    'birthdate',
    'zoneinfo',
    'locale',
    'phone_number',
    'phone_number_verified',
    'address',
    'updated_at',
    'role',
    'permissions'
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: IdentityResourcesService
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
        this.selectedUserClaims = resource.userClaims || [];
      }
    } catch (error) {
      notify('Failed to load identity resource', 'error', 3000);
      console.error('Error loading identity resource:', error);
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
      this.resource.userClaims = this.selectedUserClaims;

      if (this.isEditMode) {
        await this.service.dataSource.store().update(this.resource.id!, this.resource);
        notify('Identity resource updated successfully', 'success', 2000);
      } else {
        await this.service.dataSource.store().insert(this.resource);
        notify('Identity resource created successfully', 'success', 2000);
      }

      this.router.navigate(['/identity-resources']);
    } catch (error) {
      notify(`Failed to ${this.isEditMode ? 'update' : 'create'} identity resource`, 'error', 3000);
      console.error('Error saving identity resource:', error);
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
      this.router.navigate(['/identity-resources']);
    }
  }

  private getEmptyResource(): IdentityResource {
    return {
      name: '',
      displayName: '',
      description: '',
      enabled: true,
      required: false,
      emphasize: false,
      showInDiscoveryDocument: true,
      userClaims: [],
      nonEditable: false
    };
  }
}
