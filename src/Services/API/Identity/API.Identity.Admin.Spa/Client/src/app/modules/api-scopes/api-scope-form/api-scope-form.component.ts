import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiScopesService } from '../services/api-scopes.service';
import { ApiScope } from '../models/api-scope.model';
import notify from 'devextreme/ui/notify';
import { confirm } from 'devextreme/ui/dialog';

@Component({
  selector: 'hbg-api-scope-form',
  templateUrl: './api-scope-form.component.html',
  styleUrls: ['./api-scope-form.component.scss'],
  providers: [ApiScopesService]
})
export class ApiScopeFormComponent implements OnInit {
  scopeId: string | null = null;
  isEditMode = false;
  isLoading = false;
  isSaving = false;

  scope: ApiScope = this.getEmptyScope();

  // Multi-select items
  selectedUserClaims: string[] = [];

  // Common API claims
  availableUserClaims = [
    'sub',
    'name',
    'email',
    'role',
    'permissions',
    'department',
    'organization',
    'tenant_id',
    'scope',
    'client_id'
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: ApiScopesService
  ) {}

  ngOnInit() {
    this.scopeId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.scopeId;

    if (this.isEditMode) {
      this.loadScope();
    }
  }

  async loadScope() {
    if (!this.scopeId) return;

    this.isLoading = true;
    try {
      const scope = await this.service.dataSource.store().byKey(this.scopeId);
      if (scope) {
        this.scope = scope;
        this.selectedUserClaims = scope.userClaims || [];
      }
    } catch (error) {
      notify('Failed to load API scope', 'error', 3000);
      console.error('Error loading API scope:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async onSave() {
    // Validate required fields
    if (!this.scope.name) {
      notify('Name is required', 'warning', 3000);
      return;
    }

    this.isSaving = true;
    try {
      // Update arrays from UI
      this.scope.userClaims = this.selectedUserClaims;

      if (this.isEditMode) {
        await this.service.dataSource.store().update(this.scope.id!, this.scope);
        notify('API scope updated successfully', 'success', 2000);
      } else {
        await this.service.dataSource.store().insert(this.scope);
        notify('API scope created successfully', 'success', 2000);
      }

      this.router.navigate(['/api-scopes']);
    } catch (error) {
      notify(`Failed to ${this.isEditMode ? 'update' : 'create'} API scope`, 'error', 3000);
      console.error('Error saving API scope:', error);
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
      this.router.navigate(['/api-scopes']);
    }
  }

  private getEmptyScope(): ApiScope {
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
