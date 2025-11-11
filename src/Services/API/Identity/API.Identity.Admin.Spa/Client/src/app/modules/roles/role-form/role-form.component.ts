import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RolesService } from '../services/roles.service';
import { Role } from '../models/role.model';
import notify from 'devextreme/ui/notify';
import { confirm } from 'devextreme/ui/dialog';

@Component({
  selector: 'hbg-role-form',
  templateUrl: './role-form.component.html',
  styleUrls: ['./role-form.component.scss'],
  providers: [RolesService]
})
export class RoleFormComponent implements OnInit {
  roleId: string | null = null;
  isEditMode = false;
  isLoading = false;
  isSaving = false;

  role: Role = this.getEmptyRole();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: RolesService
  ) {}

  ngOnInit() {
    this.roleId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.roleId;

    if (this.isEditMode) {
      this.loadRole();
    }
  }

  async loadRole() {
    if (!this.roleId) return;

    this.isLoading = true;
    try {
      const role = await this.service.dataSource.store().byKey(this.roleId);
      if (role) {
        this.role = role;
      }
    } catch (error) {
      notify('Failed to load role', 'error', 3000);
      console.error('Error loading role:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async onSave() {
    // Validate required fields
    if (!this.role.name) {
      notify('Role Name is required', 'warning', 3000);
      return;
    }

    this.isSaving = true;
    try {
      if (this.isEditMode) {
        await this.service.dataSource.store().update(this.role.id!, this.role);
        notify('Role updated successfully', 'success', 2000);
      } else {
        await this.service.dataSource.store().insert(this.role);
        notify('Role created successfully', 'success', 2000);
      }

      this.router.navigate(['/roles']);
    } catch (error) {
      notify(`Failed to ${this.isEditMode ? 'update' : 'create'} role`, 'error', 3000);
      console.error('Error saving role:', error);
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
      this.router.navigate(['/roles']);
    }
  }

  private getEmptyRole(): Role {
    return {
      name: '',
      description: '',
      claims: []
    };
  }
}
