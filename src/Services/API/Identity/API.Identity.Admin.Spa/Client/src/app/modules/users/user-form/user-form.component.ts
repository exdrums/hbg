import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UsersService } from '../services/users.service';
import { User } from '../models/user.model';
import notify from 'devextreme/ui/notify';
import { confirm } from 'devextreme/ui/dialog';

@Component({
  selector: 'hbg-user-form',
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss'],
  providers: [UsersService]
})
export class UserFormComponent implements OnInit {
  userId: string | null = null;
  isEditMode = false;
  isLoading = false;
  isSaving = false;

  user: User = this.getEmptyUser();

  // Multi-select items
  selectedRoles: string[] = [];

  // Available roles (these should be fetched from API in production)
  availableRoles = [
    'Administrator',
    'User',
    'Manager',
    'Developer',
    'Support'
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: UsersService
  ) {}

  ngOnInit() {
    this.userId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.userId;

    if (this.isEditMode) {
      this.loadUser();
    }
  }

  async loadUser() {
    if (!this.userId) return;

    this.isLoading = true;
    try {
      const user = await this.service.dataSource.store().byKey(this.userId);
      if (user) {
        this.user = user;
        this.selectedRoles = user.roles || [];
      }
    } catch (error) {
      notify('Failed to load user', 'error', 3000);
      console.error('Error loading user:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async onSave() {
    // Validate required fields
    if (!this.user.username || !this.user.email) {
      notify('Username and Email are required', 'warning', 3000);
      return;
    }

    this.isSaving = true;
    try {
      // Update arrays from UI
      this.user.roles = this.selectedRoles;

      if (this.isEditMode) {
        await this.service.dataSource.store().update(this.user.id!, this.user);
        notify('User updated successfully', 'success', 2000);
      } else {
        await this.service.dataSource.store().insert(this.user);
        notify('User created successfully', 'success', 2000);
      }

      this.router.navigate(['/users']);
    } catch (error) {
      notify(`Failed to ${this.isEditMode ? 'update' : 'create'} user`, 'error', 3000);
      console.error('Error saving user:', error);
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
      this.router.navigate(['/users']);
    }
  }

  private getEmptyUser(): User {
    return {
      username: '',
      email: '',
      emailConfirmed: false,
      phoneNumber: '',
      phoneNumberConfirmed: false,
      firstName: '',
      lastName: '',
      displayName: '',
      isActive: true,
      lockoutEnabled: false,
      twoFactorEnabled: false,
      roles: [],
      claims: []
    };
  }
}
